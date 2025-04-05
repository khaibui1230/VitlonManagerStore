using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace QuanVitLonManager.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendReservationNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveReservationNotification", message);
        }

        public async Task SendOrderNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderNotification", message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}