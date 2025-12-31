using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VibeTogether.Server.Models
{
    public class PlaybackState
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public string TrackUrl { get; set; } = null!;
        public bool IsPlaying { get; set; }
        public double Position { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public string ControlledByUserId { get; set; } = null!;
    }
}
