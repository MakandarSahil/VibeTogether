namespace VibeTogether.Server.DTOs.Chat
{
    public class ChatMessageDto
    {
        public string RoomId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
