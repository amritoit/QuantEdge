using Microsoft.AspNetCore.SignalR;
using QuantEdge.Server.Models;

namespace QuantEdge.Server.Hubs
{
    public class ChatHub : Hub
    {
        // Track active users (in production, use distributed cache like Redis)
        private static readonly Dictionary<string, string> ConnectedUsers = new();

        public async Task SendMessage(string user, string message)
        {
            var messageId = Guid.NewGuid().ToString();
            
            var chatMessage = new ChatMessage
            {
                User = user,
                Message = message,
                Timestamp = DateTime.UtcNow,
                MessageId = messageId,
                Type = MessageType.User
            };

            // Broadcast message to all clients
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);

            // Send delivery confirmation to sender
            await Clients.Caller.SendAsync("MessageDelivered", new
            {
                MessageId = messageId,
                Status = "Delivered",
                Timestamp = DateTime.UtcNow
            });

            // Auto-responses based on keywords
            await HandleAutoResponses(user, message);
        }

        private async Task HandleAutoResponses(string user, string message)
        {
            var lowerMessage = message.ToLower();

            // Greeting response
            if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi"))
            {
                var response = new ChatMessage
                {
                    User = "ChatBot",
                    Message = $"Hello {user}! How can I help you today?",
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = MessageType.Bot
                };
                await Task.Delay(500); // Simulate thinking
                await Clients.All.SendAsync("ReceiveMessage", response);
            }

            // Help command
            if (lowerMessage.StartsWith("/help"))
            {
                var helpResponse = new ChatMessage
                {
                    User = "System",
                    Message = @"📋 Available Commands:
/help - Show this help message
/users - Show online users count
/time - Show current server time
/clear - Clear your chat (client-side)
                    
Try saying 'hello' to get a bot response!",
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = MessageType.System
                };
                await Clients.Caller.SendAsync("ReceiveMessage", helpResponse);
            }

            // Users command
            if (lowerMessage.StartsWith("/users"))
            {
                var usersResponse = new ChatMessage
                {
                    User = "System",
                    Message = $"👥 Currently {ConnectedUsers.Count} user(s) online",
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = MessageType.System
                };
                await Clients.Caller.SendAsync("ReceiveMessage", usersResponse);
            }

            // Time command
            if (lowerMessage.StartsWith("/time"))
            {
                var timeResponse = new ChatMessage
                {
                    User = "System",
                    Message = $"🕐 Server time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = MessageType.System
                };
                await Clients.Caller.SendAsync("ReceiveMessage", timeResponse);
            }

            // Question response
            if (lowerMessage.Contains("?"))
            {
                var questionResponse = new ChatMessage
                {
                    User = "ChatBot",
                    Message = "🤔 That's an interesting question! Our team will get back to you soon.",
                    Timestamp = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = MessageType.Bot
                };
                await Task.Delay(800);
                await Clients.All.SendAsync("ReceiveMessage", questionResponse);
            }
        }

        public async Task UserTyping(string user)
        {
            await Clients.Others.SendAsync("UserTyping", user);
        }

        public override async Task OnConnectedAsync()
        {
            // Add user to connected users
            var connectionId = Context.ConnectionId;
            
            // Send welcome message to the newly connected user
            var welcomeMessage = new ChatMessage
            {
                User = "System",
                Message = $"🎉 Welcome to the chat! You're connected as {connectionId.Substring(0, 8)}...",
                Timestamp = DateTime.UtcNow,
                MessageId = Guid.NewGuid().ToString(),
                Type = MessageType.System
            };
            await Clients.Caller.SendAsync("ReceiveMessage", welcomeMessage);

            // Send tips message
            var tipsMessage = new ChatMessage
            {
                User = "System",
                Message = "💡 Tip: Type /help to see available commands",
                Timestamp = DateTime.UtcNow,
                MessageId = Guid.NewGuid().ToString(),
                Type = MessageType.System
            };
            await Clients.Caller.SendAsync("ReceiveMessage", tipsMessage);

            // Notify all users
            ConnectedUsers[connectionId] = connectionId;
            await Clients.All.SendAsync("UserConnected", connectionId);
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            // Remove user from connected users
            ConnectedUsers.Remove(connectionId);

            // Notify all users about disconnection
            var leaveMessage = new ChatMessage
            {
                User = "System",
                Message = $"👋 A user has left the chat. {ConnectedUsers.Count} user(s) remaining.",
                Timestamp = DateTime.UtcNow,
                MessageId = Guid.NewGuid().ToString(),
                Type = MessageType.System
            };
            await Clients.All.SendAsync("ReceiveMessage", leaveMessage);

            await Clients.All.SendAsync("UserDisconnected", connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}