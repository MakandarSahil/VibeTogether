using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VibeTogether.Server.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string RoomID { get; set; } = null!;
        public string UserID { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
