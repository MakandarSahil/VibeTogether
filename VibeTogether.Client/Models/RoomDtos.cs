namespace VibeTogether.Client.Models.Rooms
{
    public class CreateRoomRequest
    {
        public string RoomName { get; set; } = "";
    }

    public class CreateRoomResponse
    {
        public string RoomId { get; set; } = "";
        public string RoomCode { get; set; } = "";
        public string HostUserId { get; set; } = "";
    }

    public class JoinRoomRequest
    {
        public string RoomCode { get; set; } = "";
    }
}
