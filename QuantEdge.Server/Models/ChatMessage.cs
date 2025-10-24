namespace QuantEdge.Server.Models
{
    public class ChatMessage
    {
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? MessageId { get; set; }
        public MessageType Type { get; set; } = MessageType.User;
    }

    public enum MessageType
    {
        User,
        System,
        Bot,
        Error,
        Success
    }
}
