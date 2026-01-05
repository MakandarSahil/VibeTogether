using VibeTogether.Client.Models.Rooms;

namespace VibeTogether.Client.Services
{
    public class RoomService
    {
        private readonly HttpClient _http;
        public RoomService(HttpClient http)
        {
            _http = http;
        }


        public async Task<CreateRoomResponse> CreateRoomAsync(string roomName)
        {
            if(string.IsNullOrEmpty(roomName))
                throw new ArgumentNullException(nameof(roomName));

            var response = await _http.PostAsJsonAsync(
                "api/rooms/create",
                new CreateRoomRequest { RoomName = roomName}
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to create room");

            return (await response.Content.ReadFromJsonAsync<CreateRoomResponse>())!;
        }

        public async Task JoinRoomAsync(string roomCode)
        {
            if(string.IsNullOrEmpty(roomCode))
                throw new ArgumentNullException(nameof(roomCode));

            var response = await _http.PostAsJsonAsync(
                "api/rooms/join",
                new JoinRoomRequest { RoomCode = roomCode }
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to join the room");
        }
    }
}
