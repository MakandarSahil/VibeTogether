using Microsoft.AspNetCore.SignalR;
using VibeTogether.Server.Services;

namespace VibeTogether.Server.Hubs
{
    public class RoomHub : Hub
    {
        private readonly PresenceService _presence;
        private readonly RoomPermissionService _permissions;
        private readonly ChatService _chat;
        private readonly MusicSyncService _music;

        public RoomHub(
            PresenceService presence,
            RoomPermissionService permissions,
            ChatService chat,
            MusicSyncService music)
        {
            _presence = presence;
            _permissions = permissions;
            _chat = chat;
            _music = music;
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            if (Context.UserIdentifier != null)
                _presence.RemoveUser(Context.UserIdentifier);

            await base.OnDisconnectedAsync(ex);
        }

        // -------- ROOM --------

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserJoined", Context.UserIdentifier);
        }

        public async Task ActivateRoom(string roomId)
        {
            _presence.SetActiveRoom(Context.UserIdentifier!, roomId);
            await Clients.Caller.SendAsync("ActiveRoomChanged", roomId);
        }

        // -------- CHAT --------

        public async Task SendMessage(string roomId, string message)
        {
            var dto = await _chat.SaveMessageAsync(
                roomId,
                Context.UserIdentifier!,
                Context.User!.Identity!.Name!,
                message
            );

            await Clients.Group(roomId).SendAsync("ReceiveMessage", dto);
        }

        // -------- HOST / CO-HOST --------

        public async Task AddCoHost(string roomId, string targetUserId)
        {
            var room = await _permissions.GetRoomAsync(roomId);
            _permissions.EnsureHost(room, Context.UserIdentifier!);

            room.CoHostUserIds.Add(targetUserId);
            await Clients.Group(roomId).SendAsync("CoHostAdded", targetUserId);
        }

        // -------- MUSIC --------

        public async Task PlayTrack(string roomId, string trackUrl)
        {
            var room = await _permissions.GetRoomAsync(roomId);
            _permissions.EnsureController(room, Context.UserIdentifier!);

            if (_presence.GetActiveRoom(Context.UserIdentifier!) != roomId)
                throw new HubException("Room not active");

            var state = await _music.Play(roomId, trackUrl, Context.UserIdentifier!);

            await Clients.Group(roomId).SendAsync("MusicPlay", new
            {
                trackUrl = state.TrackUrl,
                position = state.Position,
                serverTime = state.LastUpdatedUtc
            });
        }
    }
}
