using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace MyFirstBot
{
    public class MyFirstBot : IBot
{
    public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (turnContext.Activity.Type == ActivityTypes.Message) 
        {
            var responseMessage = string.IsNullOrEmpty(turnContext.Activity.Text) 
                                        ? "Hello" 
                                        : $"You said: {turnContext.Activity.Text}";

            await turnContext.SendActivityAsync(responseMessage);
        }
    }
}
}