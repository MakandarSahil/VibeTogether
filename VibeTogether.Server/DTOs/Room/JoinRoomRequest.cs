namespace VibeTogether.Server.DTOs.Room
{
    public class JoinRoomRequest
    {
        public string UserId { get; set; } = null!;
        public string RoomCode { get; set; } = null!;
    }
}
