using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MyFirstBot.Dialogs;
using MyFirstBot.Models;

namespace MyFirstBot
{
    public class MyFirstBot : IBot
    {
        private readonly DialogSet _dialogs;
        private readonly ConversationState _conversationState;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        private readonly BotServices _services;

        private static readonly string LuisConfiguration = "LuisBot";

        private const string GreetingIntent = "greeting";

        private const string BookingIntent = "booking";

        private const string NoneIntent = "None";

        public MyFirstBot(BotServices services, ConversationState conversationState, UserState userstate)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            _userState = userstate ?? throw new ArgumentNullException(nameof(userstate));

            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _userProfileAccessor = _userState.CreateProperty<UserProfile>(nameof(UserProfile));

            _dialogs = new DialogSet(_dialogStateAccessor);
            _dialogs.Add(new BookHotelDialog(_userState, _userProfileAccessor));
            _dialogs.Add(new WhoAreYouDialog(_userState, _userProfileAccessor));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var dialogTurnResult = await dc.ContinueDialogAsync(cancellationToken);

                UserProfile userProfile = await _userProfileAccessor.GetAsync(turnContext, () => null, cancellationToken);

                var luisResults = await _services.LuisServices[LuisConfiguration].RecognizeAsync(dc.Context, cancellationToken);
                var topScoringIntent = luisResults?.GetTopScoringIntent();
                var topIntent = topScoringIntent.Value.intent;

                if (dialogTurnResult.Status is DialogTurnStatus.Empty)
                {
                    switch (topIntent)
                    {
                        case GreetingIntent:
                            if (userProfile is null)
                            {
                                await dc.BeginDialogAsync(WhoAreYouDialog.Name, null, cancellationToken);
                            }
                            else
                            {
                                await turnContext.SendActivityAsync($"Hello {userProfile.Name}. Nice to meet you again!");
                            }

                            break;
                        case BookingIntent:
                            await dc.BeginDialogAsync(BookHotelDialog.Name, null, cancellationToken);
                            break;

                        case NoneIntent:
                        default:
                            await dc.Context.SendActivityAsync("I didn't understand what you just said to me.");
                            break;
                    }
                }
            }


            await _conversationState.SaveChangesAsync(turnContext);
        }
    }
}