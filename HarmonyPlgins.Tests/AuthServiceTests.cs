using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlgins.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
        {
            await using var dbContext = CreateDbContext();
            var passwordService = new PasswordService();
            var user = new User
            {
                Name = "Cliente Teste",
                Email = "cliente@example.com",
                PasswordHash = passwordService.HashPassword("senha123"),
                Role = UserRole.Customer
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));

            var response = await authService.LoginAsync(new LoginRequest
            {
                Email = "  CLIENTE@EXAMPLE.COM  ",
                Password = "senha123"
            });

            Assert.Equal("token-de-teste", response.Token);
            Assert.Equal(user.Id, response.User.Id);
            Assert.Equal(user.Email, response.User.Email);
            Assert.Equal("Customer", response.User.Role);
        }

        [Fact]
        public async Task LoginAsync_WithUnknownEmail_ThrowsUnauthorizedAccessException()
        {
            await using var dbContext = CreateDbContext();
            var authService = new AuthService(dbContext, new PasswordService(), new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(new LoginRequest
                {
                    Email = "inexistente@example.com",
                    Password = "senha123"
                }));

            Assert.Equal("Email ou senha inválidos.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            await using var dbContext = CreateDbContext();
            var passwordService = new PasswordService();
            dbContext.Users.Add(new User
            {
                Name = "Cliente Teste",
                Email = "cliente@example.com",
                PasswordHash = passwordService.HashPassword("senha-correta")
            });
            await dbContext.SaveChangesAsync();

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(new LoginRequest
                {
                    Email = "cliente@example.com",
                    Password = "senha-incorreta"
                }));

            Assert.Equal("Email ou senha inválidos.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithBlockedUser_ThrowsUnauthorizedAccessException()
        {
            await using var dbContext = CreateDbContext();
            var passwordService = new PasswordService();
            dbContext.Users.Add(new User
            {
                Name = "Cliente Bloqueado",
                Email = "bloqueado@example.com",
                PasswordHash = passwordService.HashPassword("uma-senha-longa-de-teste"),
                Status = UserStatus.Blocked
            });
            await dbContext.SaveChangesAsync();

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.LoginAsync(new LoginRequest
                {
                    Email = "bloqueado@example.com",
                    Password = "uma-senha-longa-de-teste"
                }));

            Assert.Equal("Sua conta está bloqueada.", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_CreatesActiveCustomerWithHashedPassword()
        {
            await using var dbContext = CreateDbContext();
            var authService = new AuthService(dbContext, new PasswordService(), new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));

            var response = await authService.RegisterAsync(new RegisterUserRequest
            {
                Name = "  Novo Cliente  ",
                Email = "  NOVO@EXAMPLE.COM ",
                Password = "uma-senha-longa-de-teste"
            });

            var user = await dbContext.Users.SingleAsync();
            Assert.Equal(response.Id, user.Id);
            Assert.Equal("Novo Cliente", user.Name);
            Assert.Equal("novo@example.com", user.Email);
            Assert.Equal(UserRole.Customer, user.Role);
            Assert.Equal(UserStatus.Active, user.Status);
            Assert.NotEqual("uma-senha-longa-de-teste", user.PasswordHash);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsConflictException()
        {
            await using var dbContext = CreateDbContext();
            var authService = new AuthService(dbContext, new PasswordService(), new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));
            var request = new RegisterUserRequest
            {
                Name = "Cliente",
                Email = "repetido@example.com",
                Password = "uma-senha-longa-de-teste"
            };
            await authService.RegisterAsync(request);

            await Assert.ThrowsAsync<ConflictException>(() => authService.RegisterAsync(request));
        }

        [Fact]
        public async Task LoginAsync_StoresOnlyTheHashOfTheRefreshToken()
        {
            await using var dbContext = CreateDbContext();
            var authService = await CreateServiceWithUserAsync(dbContext);

            var response = await authService.LoginAsync(ValidLogin);

            var stored = await dbContext.RefreshTokens.SingleAsync();
            Assert.NotEqual(response.RefreshToken, stored.TokenHash);
            Assert.Equal(64, stored.TokenHash.Length);
            Assert.True(stored.ExpiresAt > DateTime.UtcNow.AddDays(6));
            Assert.Contains(dbContext.AuditLogs, log => log.Action == "LOGIN");
        }

        [Fact]
        public async Task RefreshAsync_RotatesTokenAndRejectsTheOldOne()
        {
            await using var dbContext = CreateDbContext();
            var authService = await CreateServiceWithUserAsync(dbContext);
            var login = await authService.LoginAsync(ValidLogin);

            var refreshed = await authService.RefreshAsync(login.RefreshToken);

            Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RefreshAsync(login.RefreshToken));
        }

        [Fact]
        public async Task LogoutAsync_RevokesRefreshToken()
        {
            await using var dbContext = CreateDbContext();
            var authService = await CreateServiceWithUserAsync(dbContext);
            var login = await authService.LoginAsync(ValidLogin);

            await authService.LogoutAsync(login.RefreshToken);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RefreshAsync(login.RefreshToken));
        }

        [Fact]
        public async Task RefreshAsync_WithBlockedUser_ThrowsUnauthorizedAccessException()
        {
            await using var dbContext = CreateDbContext();
            var authService = await CreateServiceWithUserAsync(dbContext);
            var login = await authService.LoginAsync(ValidLogin);
            (await dbContext.Users.SingleAsync()).Status = UserStatus.Blocked;
            await dbContext.SaveChangesAsync();

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RefreshAsync(login.RefreshToken));

            Assert.Equal("Sua conta está bloqueada.", exception.Message);
        }

        private static readonly LoginRequest ValidLogin = new()
        {
            Email = "cliente@example.com",
            Password = "uma-senha-longa-de-teste"
        };

        private static async Task<AuthService> CreateServiceWithUserAsync(AppDbContext dbContext)
        {
            var passwordService = new PasswordService();
            dbContext.Users.Add(new User
            {
                Name = "Cliente Teste",
                Email = ValidLogin.Email,
                PasswordHash = passwordService.HashPassword(ValidLogin.Password)
            });
            await dbContext.SaveChangesAsync();

            return new AuthService(dbContext, passwordService, new FakeTokenService(), new AuditService(dbContext, new HttpContextAccessor()));
        }

        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private sealed class FakeTokenService : ITokenService
        {
            public string GenerateToken(User user) => "token-de-teste";
        }
    }
}
