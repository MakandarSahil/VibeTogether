using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(roomId)
                .SendAsync("ReceiveMessage", dto);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;

            if(!string.IsNullOrEmpty(userId))
            {
                _presenceService.RemoveUser(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
