using Microsoft.AspNetCore.Components;
using VibeTogether.Client.Services;

namespace VibeTogether.Client.Pages.Rooms
{
    public class CreateRoomViewModel
    {
        private readonly RoomService _rooms;
        private readonly RoomState _state;
        private readonly NavigationManager _nav;

        public CreateRoomViewModel(
            RoomService rooms, 
            RoomState state, 
            NavigationManager nav
        )
        {
            _rooms = rooms;
            _state = state;
            _nav = nav;
        }

        public string RoomName { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public bool IsBusy { get; set; }

        public async Task CreateAsync()
        {
            Error = string.Empty;
            if (string.IsNullOrWhiteSpace(RoomName))
            {
                Error = "Room name is required";
                return;
            }
            try
            {
                IsBusy = true;
                var result = await _rooms.CreateRoomAsync(RoomName);
                _state.SetRoom(result.RoomId, result.RoomCode, RoomName);
                _nav.NavigateTo($"/rooms/{result.RoomId}");
            }
            catch ( Exception ex )
            {
                Error = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
