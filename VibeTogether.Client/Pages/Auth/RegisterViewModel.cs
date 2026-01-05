using Microsoft.AspNetCore.Components;
using VibeTogether.Client.Auth;
using VibeTogether.Client.Services;

namespace VibeTogether.Client.Pages.Auth
{
    public class RegisterViewModel
    {
        private readonly AuthService _auth;
        private readonly JwtAuthStateProvider _authState;
        private readonly NavigationManager _nav;

        public RegisterViewModel(
            AuthService auth,
            JwtAuthStateProvider authState,
            NavigationManager nav)
        {
            _auth = auth;
            _authState = authState;
            _nav = nav;
        }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public async Task RegisterAsync()
        {
            Error = string.Empty;
            try
            {
                await _auth.RegisterAsync(Username, Email, Password);
                _authState.SetAuthenticated(_auth.JwtToken!);
                _nav.NavigateTo("/rooms/create");
            }
            catch (Exception ex)
            {
                Error = ex.ToString();
            }
        }
    }
}
