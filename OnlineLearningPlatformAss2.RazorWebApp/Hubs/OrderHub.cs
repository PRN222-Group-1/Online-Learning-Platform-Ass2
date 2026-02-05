using Microsoft.AspNetCore.SignalR;

namespace OnlineLearningPlatformAss2.RazorWebApp.Hubs;

public class OrderHub : Hub
{
    public async Task SendOrderExpired(Guid orderId)
    {
        await Clients.All.SendAsync("OrderExpired", orderId);
    }

    public async Task SendLogMessage(string message)
    {
        await Clients.All.SendAsync("LogMessage", message);
    }
}
