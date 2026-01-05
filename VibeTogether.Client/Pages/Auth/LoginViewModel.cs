using Microsoft.AspNetCore.Components;
using VibeTogether.Client.Auth;
using VibeTogether.Client.Services;

namespace VibeTogether.Client.Pages.Auth
{
    public class LoginViewModel
    {
        private readonly AuthService _auth;
        private readonly JwtAuthStateProvider _authState;
        private readonly NavigationManager _nav;

        public LoginViewModel(
             AuthService auth,
             JwtAuthStateProvider authState,
             NavigationManager nav
        )
        {
            _auth = auth;
            _authState = authState;
            _nav = nav;
        }

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;

        public async Task LoginAsync()
        {
            Error = "";

            try
            {
                await _auth.LoginAsync(Email, Password);
                _authState.SetAuthenticated(_auth.JwtToken!);
                _nav.NavigateTo("/rooms/create");
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
