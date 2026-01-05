namespace VibeTogether.Client.Services
{
    public class RoomState
    {
        public string? CurrentRoomId { get; private set; }
        public string? CurrentRoomCode {  get; private set; }
        public string? CurrentRoomName { get; private set; }

        public void SetRoom(string roomId, string roomCode, string roomName)
        {
            CurrentRoomId = roomId;
            CurrentRoomCode = roomCode;
            CurrentRoomName = roomName;
        }

        public void ClearRoom()
        {
            CurrentRoomId = null;
            CurrentRoomCode = null;
            CurrentRoomName = null;
        }
    }
}
