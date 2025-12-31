using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using VibeTogether.Server.Data;
using VibeTogether.Server.Models;

namespace VibeTogether.Server.Services
{
    public class MusicSyncService
    {
        private readonly MongoContext _context;

        public MusicSyncService(MongoContext context)
        {
            _context = context;
        }

        public async Task<PlaybackState> Play(
            string roomId,
            string trackUrl,
            string userId
        )
        {
            var state = new PlaybackState
            {
                RoomId = roomId,
                TrackUrl = trackUrl,
                IsPlaying = true,
                Position = 0,
                LastUpdatedUtc = DateTime.UtcNow,
                ControlledByUserId = userId
            };

            await _context.PlaybackStates.ReplaceOneAsync(
                s => s.RoomId == roomId,
                state,
                new ReplaceOptions { IsUpsert = true }
            );

            return state;
        }

        public async Task<PlaybackState> GetState(string roomId)
        {
            var state = await _context.PlaybackStates
                .Find(s => s.RoomId == roomId)
                .FirstOrDefaultAsync();

            if (state == null)
                throw new HubException("Nothing is playing");

            return state;
        }

        public async Task SaveState(PlaybackState state)
        {
            await _context.PlaybackStates.ReplaceOneAsync(
                s => s.RoomId == state.RoomId,
                state
            );
        }
    }
}
