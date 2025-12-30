using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VibeTogether.Server.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string RoomCode { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string HostUserId { get; set; } = null!;
        public List<string> Members { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}