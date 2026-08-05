using Microsoft.AspNetCore.SignalR;

namespace SampleBlazorServerApp.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", user, message);
    }
}