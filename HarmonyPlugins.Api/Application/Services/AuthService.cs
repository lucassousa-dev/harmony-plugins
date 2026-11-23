using System.Security.Cryptography;
using System.Text;
using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.DTOs.Users;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services
{
    public class AuthService : IAuthService
    {
        // RNF01: token de renovação válido por 7 dias (o de acesso vale 15 minutos, em Jwt:ExpirationMinutes).
        private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

        private readonly AppDbContext _appDbContext;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IAuditService _auditService;

        public AuthService(
            AppDbContext appDbContext,
            IPasswordService passwordService,
            ITokenService tokenService,
            IAuditService auditService)
        {
            _appDbContext = appDbContext;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _auditService = auditService;
        }

        public async Task<UserResponse> RegisterAsync(RegisterUserRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            bool emailAlreadyExists = await _appDbContext.Users.AnyAsync(u => u.Email == normalizedEmail);

            if (emailAlreadyExists)
                throw new ConflictException("Já existe um usuário cadastrado com este email.");

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Role = UserRole.Customer,
                Status = UserStatus.Active
            };

            await _appDbContext.Users.AddAsync(user);
            _auditService.Record("CADASTRO_USUARIO", "User", user.Id, user.Id);
            await _appDbContext.SaveChangesAsync();

            return ToResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await _appDbContext.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user is null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email ou senha inválidos.");

            // UC02 4a: checado só após a senha, para não revelar o status de contas alheias.
            if (user.Status == UserStatus.Blocked)
                throw new UnauthorizedAccessException("Sua conta está bloqueada.");

            _auditService.Record("LOGIN", "User", user.Id, user.Id);
            return await IssueTokensAsync(user);
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);
            var stored = await _appDbContext.RefreshTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");

            if (stored.User!.Status == UserStatus.Blocked)
                throw new UnauthorizedAccessException("Sua conta está bloqueada.");

            // Cada token de renovação vale uma vez: é revogado e substituído (UC02, renovar sessão).
            stored.RevokedAt = DateTime.UtcNow;
            return await IssueTokensAsync(stored.User);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);
            var stored = await _appDbContext.RefreshTokens
                .SingleOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAt == null);

            if (stored is null)
                return;

            stored.RevokedAt = DateTime.UtcNow;
            _auditService.Record("LOGOUT", "User", stored.UserId, stored.UserId);
            await _appDbContext.SaveChangesAsync();
        }

        private async Task<AuthResponse> IssueTokensAsync(User user)
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            // Só o hash vai para o banco: um vazamento da tabela não permite renovar sessões.
            _appDbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime)
            });
            await _appDbContext.SaveChangesAsync();

            return new AuthResponse
            {
                Token = _tokenService.GenerateToken(user),
                RefreshToken = refreshToken,
                User = ToResponse(user)
            };
        }

        private static string HashToken(string token) =>
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        private static UserResponse ToResponse(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        };
    }
}
