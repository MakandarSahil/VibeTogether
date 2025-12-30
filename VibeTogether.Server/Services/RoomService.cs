using MongoDB.Driver;
using VibeTogether.Server.Data;
using VibeTogether.Server.DTOs.Room;
using VibeTogether.Server.Infrastructure.Generators;
using VibeTogether.Server.Models;
using VibeTogether.Server.Services.Interfaces;

namespace VibeTogether.Server.Services
{
    public class RoomService : IRoomService
    {
        public readonly MongoContext _context;

        public RoomService(MongoContext context)
        {
            _context = context;
        }

        public async Task<CreateRoomResponse> CreateRoomAsync(string userId, CreateRoomRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoomName))
                throw new ArgumentException("Room name is required");

            string roomCode;
            do
            {
                roomCode = RoomCodeGenerator.Generate();
            }
            while (await _context.Rooms
                .Find(r => r.RoomCode == roomCode)
                .AnyAsync()
            );

            var room = new Room
            {
                RoomName = request.RoomName.Trim(),
                RoomCode = roomCode,
                HostUserId = userId,
                CoHostUserIds = new(),
                Members = new List<string> { userId }
            };

            await _context.Rooms.InsertOneAsync(room);


            return new CreateRoomResponse
            {
                RoomId = room.Id,
                RoomName = room.RoomName,
                RoomCode = room.RoomCode,
                HostUserId = room.HostUserId
            };
        }

        public async Task JoinRoomAsync(string userId, string roomCode)
        {
            var room = await _context.Rooms
                .Find(r => r.RoomCode == roomCode)
                .FirstOrDefaultAsync();

            if (room == null)
                throw new InvalidOperationException("Room not found");

            if (room.Members.Contains(userId))
                return;

            var update = Builders<Room>.Update
                .AddToSet(r => r.Members, userId);

            await _context.Rooms.UpdateOneAsync(
                r => r.Id == room.Id,
                update
            );
        }
    }
}
