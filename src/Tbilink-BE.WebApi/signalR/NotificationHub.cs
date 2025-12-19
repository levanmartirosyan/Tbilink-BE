using Microsoft.AspNetCore.SignalR;

namespace Tbilink_BE.WebApi.signalR
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
