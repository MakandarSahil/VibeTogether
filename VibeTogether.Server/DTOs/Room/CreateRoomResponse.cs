namespace VibeTogether.Server.DTOs.Room
{
    public class CreateRoomResponse
    {
        public string RoomId { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
        public string HostUserId { get; set; } = null!;
    }
}