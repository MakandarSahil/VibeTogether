using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using VibeTogether.Client.Models;

namespace VibeTogether.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly ProtectedLocalStorage _storage;
        private const string TOKEN_KEY = "auth_token";

        public string? JwtToken { get; private set; }

        public AuthService(
            IHttpClientFactory factory,
            ProtectedLocalStorage storage)
        {
            _http = factory.CreateClient("Api");
            _storage = storage;
        }

        public async Task LoginAsync(string email, string password)
        {
            var res = await _http.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequest { Email = email, Password = password });

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {error}");
            }

            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>()
                       ?? throw new Exception("Invalid response");

            await SetTokenAsync(auth.Token);
        }

        public async Task RegisterAsync(string username, string email, string password)
        {
            var res = await _http.PostAsJsonAsync(
                "/api/auth/register",
                new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                });

            if (!res.IsSuccessStatusCode)
            {
                var error = await res.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed: {error}");
            }

            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>()
                       ?? throw new Exception("Invalid response");

            await SetTokenAsync(auth.Token);
        }

        public async Task<string?> LoadTokenAsync()
        {
            try
            {
                var result = await _storage.GetAsync<string>(TOKEN_KEY);
                JwtToken = result.Success ? result.Value : null;

                if (!string.IsNullOrEmpty(JwtToken))
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", JwtToken);
                }

                return JwtToken;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Data Protection key changed - clear corrupted data
                Console.WriteLine("Could not decrypt stored token - clearing storage");
                try
                {
                    await _storage.DeleteAsync(TOKEN_KEY);
                }
                catch { /* Ignore delete errors */ }

                JwtToken = null;
                return null;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
            {
                // Called during prerendering - this is expected, ignore
                Console.WriteLine("LoadToken called during prerender - skipping");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading token: {ex.Message}");
                JwtToken = null;
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            JwtToken = null;
            _http.DefaultRequestHeaders.Authorization = null;

            try
            {
                await _storage.DeleteAsync(TOKEN_KEY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token: {ex.Message}");
            }
        }

        private async Task SetTokenAsync(string token)
        {
            JwtToken = token;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                await _storage.SetAsync(TOKEN_KEY, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing token: {ex.Message}");
                // Token is still in memory, so login will work for this session
            }
        }
    }
}