using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.DTOs.Users;
using HarmonyPlugins.Api.Application.Interfaces;

using HarmonyPlugins.Api.Infrastructure.Data;

using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;

using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthService(
            AppDbContext appDbContext,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _appDbContext = appDbContext;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            bool emailAlreadyExists = await _appDbContext.Users.AnyAsync(u => u.Email == normalizedEmail);

            if (emailAlreadyExists)
                throw new InvalidOperationException("Já existe um usuário cadastrado com este email.");

            var passwordHash = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = UserRole.Customer
            };

            await _appDbContext.Users.AddAsync(user);

            await _appDbContext.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _appDbContext.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email ou senha inválidos.");

            return new AuthResponse
            {
                Token = _tokenService.GenerateToken(user),
                User = new UserResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };
        }
    }
}
