using VibeTogether.Server.DTOs.Room;

namespace VibeTogether.Server.Services.Interfaces
{
    public interface IRoomService
    {
        Task<CreateRoomResponse> CreateRoomAsync(string userId, CreateRoomRequest request);
        Task JoinRoomAsync(string userId, string roomCode);
    }
}
