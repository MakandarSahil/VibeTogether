using MongoDB.Driver;
using VibeTogether.Server.Data;
using VibeTogether.Server.DTOs.Auth;
using VibeTogether.Server.Infrastructure.Generators;
using VibeTogether.Server.Infrastructure.Security;
using VibeTogether.Server.Models;
using VibeTogether.Server.Services.Interfaces;

namespace VibeTogether.Server.Services.Auth
{
    public class AuthServices : IAuthService
    {
        private readonly MongoContext _context;
        private readonly PasswordHashService _passwordHasher;
        private readonly JwtTokenService _jwtTokenService;
        public AuthServices(MongoContext context, PasswordHashService passwordHasher, JwtTokenService jwtTokenService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var emailExists = await _context.Users
               .Find(u => u.Email == request.Email)
               .AnyAsync();

            if (emailExists)
                throw new InvalidOperationException("Email already registered");

            var usernameExits = await _context.Users
                .Find(u => u.Username == request.Username)
                .AnyAsync();

            if (usernameExits)
                throw new InvalidOperationException("Username already registered");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new InvalidOperationException("Password not provided");

            var user = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = _passwordHasher.Hash(request.Password)
            };

            await _context.Users.InsertOneAsync(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
            };

        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Find(u => u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new InvalidOperationException("Invalid credentials");

            var isValid = _passwordHasher.Verify(
                request.Password,
                user.PasswordHash
            );

            if (!isValid)
                throw new InvalidOperationException("Invalid credentials");

            var token = _jwtTokenService.GenrateToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Token = token
            };
        }
    }
}
