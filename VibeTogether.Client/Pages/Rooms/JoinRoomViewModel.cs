using Microsoft.AspNetCore.Components;
using VibeTogether.Client.Services;

namespace VibeTogether.Client.Pages.Rooms
{
    public class JoinRoomViewModel
    {
        private readonly RoomService _rooms;
        private readonly RoomState _state;
        private readonly NavigationManager _nav;

        public JoinRoomViewModel(
            RoomService rooms,
            RoomState state,
            NavigationManager nav)
        {
            _rooms = rooms;
            _state = state;
            _nav = nav;
        }

        public string RoomCode { get; set; } = "";
        public string Error { get; set; } = "";
        public bool IsBusy { get; set; }

        public async Task JoinAsync()
        {
            Error = "";

            if (string.IsNullOrWhiteSpace(RoomCode))
            {
                Error = "Room code required";
                return;
            }

            try
            {
                IsBusy = true;

                await _rooms.JoinRoomAsync(RoomCode);

                // backend returns roomId implicitly (via membership)
                // we’ll fetch details later in dashboard
                _state.SetRoom(null!, RoomCode, "");

                _nav.NavigateTo("/rooms/dashboard");
            }
            catch (Exception ex)
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
