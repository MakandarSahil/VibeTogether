using System.Collections.Concurrent;

namespace VibeTogether.Server.Services
{
    public class PresenceService
    {
        private readonly ConcurrentDictionary<string, string?> _activeRooms = new();

        public void SetActiveRoom(string userId, string roomId)
        {
            _activeRooms[userId] = roomId;
        }

        public void ClearActiveRoom(string userId)
        {
            _activeRooms[userId] = null;
        }

        public string? GetActiveRoom(string userId)
        {
            _activeRooms.TryGetValue(userId, out var roomId);
            return roomId;
        }

        public void RemoveUser(string userId)
        {
            _activeRooms.TryRemove(userId, out _);
        }
    }
}
