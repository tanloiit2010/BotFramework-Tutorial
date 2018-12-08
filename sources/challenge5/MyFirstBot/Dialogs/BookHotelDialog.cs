using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using MyFirstBot.Models;

namespace MyFirstBot.Dialogs
{
    public class BookHotelDialog : ComponentDialog
    {
        public const string Name = "book_hotel";

        private string dialogStart => "hotel_booking_start";

        private string RoomTypePrompt => "RoomTypePrompt";

        private string StartDatePrompt => "StartDatePrompt";

        private string EndDatePrompt => "EndDatePrompt";

        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        private readonly UserState _userState;


        public BookHotelDialog(UserState userState, IStatePropertyAccessor<UserProfile> userProfileAccessor) : base(Name)
        {
            _userState = userState;
            _userProfileAccessor = userProfileAccessor;

            var waterfallSteps = new WaterfallStep[]
            {
            RoomTypeStepAsync,
            StartDateStepAsync,
            EndDateStepAsync,
            UserInfoStepAsync,
            FinalStepAsync
            };

            AddDialog(new WaterfallDialog(
               dialogStart,
               waterfallSteps));

            AddDialog(new ChoicePrompt(RoomTypePrompt));
            AddDialog(new DateTimePrompt(StartDatePrompt));
            AddDialog(new DateTimePrompt(EndDatePrompt));
            AddDialog(new WhoAreYouDialog(_userState, _userProfileAccessor));
        }

        private async Task<DialogTurnResult> RoomTypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["BookingInfo"] = new HotelBookingInfo();

            return await stepContext.PromptAsync(
                RoomTypePrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Which room type you would like to book?"),
                    RetryPrompt = MessageFactory.Text("Sorry, please choose a location from the list."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Ritzy Suite", "Superior Double", "Deluxe Twin" }),
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> StartDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ((HotelBookingInfo)stepContext.Values["BookingInfo"]).RoomType = this.GetRoomType(((FoundChoice)stepContext.Result).Value) ?? throw new InvalidOperationException("The room type is invalid");

            return await stepContext.PromptAsync(
                StartDatePrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What's your start date?"),
                    RetryPrompt = MessageFactory.Text("Sorry, please choose a location from the list.")
                },
                cancellationToken);
        }

        private async Task<DialogTurnResult> EndDateStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as IEnumerable<DateTimeResolution>;
            ((HotelBookingInfo)stepContext.Values["BookingInfo"]).StartDate = DateTime.Parse(result.First().Value);

            return await stepContext.PromptAsync(
                EndDatePrompt,
                new PromptOptions { Prompt = MessageFactory.Text("What's your end date?") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> UserInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = stepContext.Result as IEnumerable<DateTimeResolution>;
            ((HotelBookingInfo)stepContext.Values["BookingInfo"]).EndDate = DateTime.Parse(result.First().Value);

            var userInfo = await _userProfileAccessor.GetAsync(stepContext.Context, () => null, cancellationToken);

            if (userInfo is null)
            {
                return await stepContext.BeginDialogAsync(WhoAreYouDialog.Name, null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(userInfo);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var bookingInfo = (HotelBookingInfo)stepContext.Values["BookingInfo"];
            bookingInfo.UserInfo = (UserProfile)stepContext.Result;

            await stepContext.Context.SendActivityAsync($"Your booking info is:");
            await stepContext.Context.SendActivityAsync($"Name:{bookingInfo.UserInfo.Name}. Phone number: {bookingInfo.UserInfo.PhoneNumber}");
            await stepContext.Context.SendActivityAsync($"Room Type: {bookingInfo.RoomType}");
            await stepContext.Context.SendActivityAsync($"From {bookingInfo.StartDate.ToLongDateString()} To {bookingInfo.EndDate.ToLongDateString()}");

            return await stepContext.EndDialogAsync(bookingInfo);
        }

        private RoomType? GetRoomType(string result)
        {
            switch (result)
            {
                case "Ritzy Suite":
                    return RoomType.RitzySuite;
                case "Superior Double":
                    return RoomType.SuperiorDouble;
                case "Deluxe Twin":
                    return RoomType.DeluxeTwin;
                default:
                    return null;
            }
        }
    }
}