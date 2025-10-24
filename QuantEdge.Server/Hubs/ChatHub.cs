using Microsoft.AspNetCore.SignalR;
using QuantEdge.Server.Models;

namespace QuantEdge.Server.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new ChatMessage
            {
                User = user,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }

        public async Task UserTyping(string user)
        {
            await Clients.Others.SendAsync("UserTyping", user);
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}