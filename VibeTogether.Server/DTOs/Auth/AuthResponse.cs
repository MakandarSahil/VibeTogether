namespace VibeTogether.Server.DTOs.Auth
{
    public class AuthResponse
    {
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
