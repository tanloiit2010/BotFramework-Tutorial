using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MyFirstBot.Dialogs;

namespace MyFirstBot
{
    public class MyFirstBot : IBot
    {
        private readonly DialogSet _dialogs;
        private readonly ConversationState _conversationState;
        private readonly IStatePropertyAccessor<DialogState> _dialogStateAccessor;

        public MyFirstBot(ConversationState conversationState)
        {
            _conversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            _dialogStateAccessor = _conversationState.CreateProperty<DialogState>(nameof(DialogState));
            _dialogs = new DialogSet(_dialogStateAccessor);
            _dialogs.Add(new WhoAreYouDialog());
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dc = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var dialogTurnResult = await dc.ContinueDialogAsync(cancellationToken);
                
                if (dialogTurnResult.Status is DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(WhoAreYouDialog.Name, null, cancellationToken);
                }
                else if (dialogTurnResult.Status is DialogTurnStatus.Complete) 
                {
                    await turnContext.SendActivityAsync("Thank you");
                }
            }
            

            await _conversationState.SaveChangesAsync(turnContext);
        }
    }
}