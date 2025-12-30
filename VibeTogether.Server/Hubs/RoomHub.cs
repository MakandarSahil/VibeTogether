using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using System;
using VibeTogether.Server.Data;
using VibeTogether.Server.DTOs.Chat;
using VibeTogether.Server.Models;
using VibeTogether.Server.Services;

namespace VibeTogether.Server.Hubs
{
    public class RoomHub : Hub
    {
        private readonly PresenceService _presenceService;
        private readonly MongoContext _context;

        public RoomHub(PresenceService presenceService, MongoContext context)
        {
            _presenceService = presenceService;
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;

            if(string.IsNullOrEmpty(userId) )
            {
                Context.Abort();
                return;
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                _presenceService.RemoveUser(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string roomId)
        {
            var userId = Context.UserIdentifier!;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await Clients.Groups(roomId)
                .SendAsync("UserJoined", userId);
        }

        public async Task LeaveRoom(string roomId)
        {
            var userId = Context.UserIdentifier!;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            await Clients.Group(roomId)
                .SendAsync("UserLeft", userId);
        }

        public async Task SendMessage(string roomId, string message)
        {
            var userId = Context.UserIdentifier!;
            var username = Context.User!.Identity!.Name!;

            var chatMessage = new ChatMessage
            {
                RoomID = roomId,
                UserID = userId,
                Username = username,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _context.ChatMessages.InsertOneAsync(chatMessage);

            var dto = new ChatMessageDto
            {
                RoomId = roomId,
                Username = username,
                Message = message,
                Timestamp = chatMessage.Timestamp
            };

            await Clients.Group(roomId)
                .SendAsync("ReceiveMessage", dto);
        }

        public async Task ActivateRoom(string roomId)
        {
            var userId = Context.UserIdentifier!;
            _presenceService.SetActiveRoom(userId, roomId);

            await Clients.Caller.SendAsync("ActiveRoomChanged", roomId);
        }

        public Task<string?> GetActiveRoom()
        {
            var userId = Context.UserIdentifier!;
            return Task.FromResult(_presenceService.GetActiveRoom(userId));
        }

        public async Task AddCoHost(string roomId, string targetUserId)
        {
            var userId = Context.UserIdentifier!;
            var room = await GetRoom(roomId);

            if (room.HostUserId != userId)
                throw new HubException("Only host can add co-host");

            if (!room.CoHostUserIds.Contains(targetUserId))
            {
                room.CoHostUserIds.Add(targetUserId);
                await SaveRoom(room);
            }

            await Clients.Group(roomId)
                .SendAsync("CoHostAdded", targetUserId);
        }

        public async Task RemoveCoHost(string roomId, string targetUserId)
        {
            var userId = Context.UserIdentifier!;
            var room = await GetRoom(roomId);

            if (room.HostUserId != userId)
                throw new HubException("Only host can remove co-hosts");

            room.CoHostUserIds.Remove(targetUserId);
            await SaveRoom(room);

            await Clients.Group(roomId)
                .SendAsync("CoHostRemoved", targetUserId);
        }

        private async Task<Room> GetRoom(string roomId)
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


        private async Task SaveRoom(Room room)
        {
            await _context.Rooms.ReplaceOneAsync(
                r => r.Id == room.Id,
                room
            );
        }

        public async Task PlayTrack(string roomId, string trackUrl)
        {
            var userId = Context.UserIdentifier!;
            var room = await GetRoom(roomId);

            // Permission
            if (room.HostUserId != userId &&
                !room.CoHostUserIds.Contains(userId))
                throw new HubException("No permission");

            // Active room
            if (_presenceService.GetActiveRoom(userId) != roomId)
                throw new HubException("Activate room first");

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

            await Clients.Group(roomId).SendAsync("MusicPlay", new
            {
                trackUrl,
                position = 0,
                serverTime = state.LastUpdatedUtc
            });
        }


        public async Task Pause(string roomId, double position)
        {
            var userId = Context.UserIdentifier!;
            var state = await GetPlaybackState(roomId);

            EnsureController(roomId, userId);
            EnsureActiveRoom(userId, roomId);

            state.IsPlaying = false;
            state.Position = position;
            state.LastUpdatedUtc = DateTime.UtcNow;

            await SavePlaybackState(state);

            await Clients.Group(roomId).SendAsync("MusicPause", new
            {
                position,
                serverTime = state.LastUpdatedUtc
            });
        }

        public async Task Seek(string roomId, double position)
        {
            var userId = Context.UserIdentifier!;
            var state = await GetPlaybackState(roomId);

            EnsureController(roomId, userId);
            EnsureActiveRoom(userId, roomId);

            state.Position = position;
            state.LastUpdatedUtc = DateTime.UtcNow;

            await SavePlaybackState(state);

            await Clients.Group(roomId).SendAsync("MusicSeek", new
            {
                position,
                serverTime = state.LastUpdatedUtc
            });
        }


        private async Task<PlaybackState> GetPlaybackState(string roomId)
        {
            var state = await _context.PlaybackStates
                .Find(s => s.RoomId == roomId)
                .FirstOrDefaultAsync();

            if (state == null)
                throw new HubException("Nothing is playing");

            return state;
        }

        private async Task SavePlaybackState(PlaybackState state)
        {
            await _context.PlaybackStates.ReplaceOneAsync(
                s => s.RoomId == state.RoomId,
                state
            );
        }

        private void EnsureController(string roomId, string userId)
        {
            var room = _context.Rooms.Find(r => r.Id == roomId).FirstOrDefault();
            if (room == null ||
                (room.HostUserId != userId &&
                 !room.CoHostUserIds.Contains(userId)))
                throw new HubException("No permission");
        }

        private void EnsureActiveRoom(string userId, string roomId)
        {
            if (_presenceService.GetActiveRoom(userId) != roomId)
                throw new HubException("Room not active");
        }


    }
}
