using Microsoft.AspNetCore.Identity;

namespace VibeTogether.Server.Infrastructure.Security
{
    public class PasswordHashService
    {
        private readonly PasswordHasher<object> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public bool Verify(string password, string hashedPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                null!,
                hashedPassword,
                password
            );

            return result == PasswordVerificationResult.Success;
        }
    }
}