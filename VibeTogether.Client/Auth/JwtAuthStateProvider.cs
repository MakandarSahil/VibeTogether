using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VibeTogether.Client.Services;

namespace VibeTogether.Client.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthService _auth;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private ClaimsPrincipal? _currentUser;
        private bool _initialized = false;

        public JwtAuthStateProvider(AuthService auth)
        {
            _auth = auth;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return new AuthenticationState(_currentUser ?? _anonymous);
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                var token = await _auth.LoadTokenAsync();

                if (string.IsNullOrWhiteSpace(token))
                {
                    _currentUser = _anonymous;
                }
                else
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);

                        // Check if token is expired
                        if (jwt.ValidTo < DateTime.UtcNow)
                        {
                            Console.WriteLine("Token expired");
                            await _auth.LogoutAsync();
                            _currentUser = _anonymous;
                        }
                        else
                        {
                            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                            _currentUser = new ClaimsPrincipal(identity);
                            Console.WriteLine($"Token loaded successfully, expires: {jwt.ValidTo}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Invalid token: {ex.Message}");
                        await _auth.LogoutAsync();
                        _currentUser = _anonymous;
                    }
                }

                _initialized = true;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth: {ex.Message}");
                _currentUser = _anonymous;
                _initialized = true;
            }
        }

        public void SetAuthenticated(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(jwt.Claims, "jwt");
                _currentUser = new ClaimsPrincipal(identity);
                _initialized = true;
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting authenticated state: {ex}");
                _currentUser = _anonymous;
            }
        }

        public async Task SetLoggedOutAsync()
        {
            _currentUser = _anonymous;
            _initialized = true;
            await _auth.LogoutAsync();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}