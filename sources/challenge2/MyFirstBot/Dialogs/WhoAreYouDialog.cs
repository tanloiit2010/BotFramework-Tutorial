using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace MyFirstBot.Dialogs
{
    public class WhoAreYouDialog : ComponentDialog
    {
        public const string Name = "Who_are_you";

        private string NamePrompt => "NamePrompt";

        private string AgePrompt => "AgePrompt";

        private string PhoneNumberPrompt => "PhoneNumberPrompt";

        private string dialogStart => "Who_are_you_start";

        public WhoAreYouDialog() : base(Name)
        {
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                AgeStepAsync,
                PhoneNumberStepAsync,
                FinalStepAsync
            };

            AddDialog(new WaterfallDialog(
               dialogStart,
               waterfallSteps));

            AddDialog(new TextPrompt(NamePrompt));
            AddDialog(new NumberPrompt<int>(AgePrompt));
            AddDialog(new NumberPrompt<int>(PhoneNumberPrompt));
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                NamePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                AgePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(
                PhoneNumberPrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your phone number.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync();
        }
    }
}