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

        public MyFirstBot(ConversationState conversationState, UserState userstate)
        {
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

                if (dialogTurnResult.Status is DialogTurnStatus.Empty)
                {
                    var text = turnContext.Activity.Text.ToLower();

                    switch (text)
                    {
                        case "hi":
                        case "hello":
                        case "hey":
                            if (userProfile is null)
                            {
                                await dc.BeginDialogAsync(WhoAreYouDialog.Name, null, cancellationToken);
                            }
                            else
                            {
                                await turnContext.SendActivityAsync($"Hello {userProfile.Name}. Nice to meet you again!");
                            }

                            break;
                        case "I would like to book a room":
                        case "book room, please":
                            await dc.BeginDialogAsync(BookHotelDialog.Name, null, cancellationToken);
                            break;
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