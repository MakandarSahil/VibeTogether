using MongoDB.Driver;
using VibeTogether.Server.Configuration;
using VibeTogether.Server.Models;
using Microsoft.Extensions.Options;

namespace VibeTogether.Server.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;
        
        public MongoContext(IOptions<MongoSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);

            CreateIndexes();
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");
        public IMongoCollection<ChatMessage> ChatMessages => _database.GetCollection<ChatMessage>("ChatMessages");
        public IMongoCollection<PlaybackState> PlaybackStates => _database.GetCollection<PlaybackState>("PlaybackStates");

        public void CreateIndexes()
        {
            var emailIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true}
            );
            Users.Indexes.CreateOne(emailIndex);

            var usernameIndex = new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true}
            );
            Users.Indexes.CreateOne(usernameIndex);

            var roomCodeIndex = new CreateIndexModel<Room>(
                Builders<Room>.IndexKeys.Ascending(r => r.RoomCode),
                new CreateIndexOptions { Unique = true}
            );
            Rooms.Indexes.CreateOne(roomCodeIndex);

        }
        
    }
}
