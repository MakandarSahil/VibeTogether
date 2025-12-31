using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using VibeTogether.Server.Data;
using VibeTogether.Server.Models;

namespace VibeTogether.Server.Services
{
    public class RoomPermissionService
    {
        private readonly MongoContext _context;

        public RoomPermissionService(MongoContext context)
        {
            _context = context;
        }

        public async Task<Room> GetRoomAsync(string roomId)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(roomId, out _))
                throw new HubException("Invalid room id");

            var room = await _context.Rooms
                .Find(r => r.Id == roomId)
                .FirstOrDefaultAsync();

            if (room == null)
                throw new HubException("Room not found");

            return room;
        }

        public void EnsureHost(Room room, string userId)
        {
            if (room.HostUserId != userId)
                throw new HubException("Only host allowed");
        }

        public void EnsureController(Room room, string userId)
        {
            if (room.HostUserId != userId &&
                !room.CoHostUserIds.Contains(userId))
                throw new HubException("No permission");
        }
    }
}
