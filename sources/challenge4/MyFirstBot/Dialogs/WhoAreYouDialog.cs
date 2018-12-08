using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using MyFirstBot.Models;

namespace MyFirstBot.Dialogs
{
    public class WhoAreYouDialog : ComponentDialog
    {
        public const string Name = "Who_are_you";

        private string NamePrompt => "NamePrompt";

        private string AgePrompt => "AgePrompt";

        private string PhoneNumberPrompt => "PhoneNumberPrompt";

        private string dialogStart => "Who_are_you_start";

        private readonly UserState _userState;

        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public WhoAreYouDialog(UserState userState, IStatePropertyAccessor<UserProfile> userProfileAccessor) : base(Name)
        {
            this._userState = userState;
            this._userProfileAccessor = userProfileAccessor;
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
            stepContext.Values["UserInfo"] = new UserProfile();

            return await stepContext.PromptAsync(
                NamePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var name = (string)stepContext.Result;
            ((UserProfile)stepContext.Values["UserInfo"]).Name = name;

            return await stepContext.PromptAsync(
                AgePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNumberStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var age = (int)stepContext.Result;
            ((UserProfile)stepContext.Values["UserInfo"]).Age = age;

            return await stepContext.PromptAsync(
                PhoneNumberPrompt,
                new PromptOptions { Prompt = MessageFactory.Text("Please enter your phone number.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(
        WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
        {
            var userProfile = ((UserProfile)stepContext.Values["UserInfo"]);
            var phoneNumber = stepContext.Result.ToString();
            userProfile.PhoneNumber = phoneNumber;

            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);
            await _userState.SaveChangesAsync(stepContext.Context, false, cancellationToken);

            await stepContext.Context.SendActivityAsync("Thank you.");

            return await stepContext.EndDialogAsync(stepContext.Values["UserInfo"]);
        }
    }
}