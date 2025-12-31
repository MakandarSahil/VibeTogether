using VibeTogether.Server.Data;
using VibeTogether.Server.DTOs.Chat;
using VibeTogether.Server.Models;

namespace VibeTogether.Server.Services
{
    public class ChatService
    {
        private readonly MongoContext _context;

        public ChatService(MongoContext context)
        {
            _context = context;
        }

        public async Task<ChatMessageDto> SaveMessageAsync(
            string roomId,
            string userId,
            string username,
            string message
        )
        {
            var chat = new ChatMessage
            {
                RoomID = roomId,
                UserID = userId,
                Username = username,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _context.ChatMessages.InsertOneAsync(chat);

            return new ChatMessageDto
            {
                RoomId = roomId,
                Username = username,
                Message = message,
                Timestamp = chat.Timestamp
            };
        }
    }
}
