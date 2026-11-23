using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Infrastructure.Data;
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

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService());

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
            var authService = new AuthService(
                dbContext,
                new PasswordService(),
                new FakeTokenService());

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

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService());

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

            var authService = new AuthService(dbContext, passwordService, new FakeTokenService());

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
            var authService = new AuthService(dbContext, new PasswordService(), new FakeTokenService());

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
        public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            await using var dbContext = CreateDbContext();
            var authService = new AuthService(dbContext, new PasswordService(), new FakeTokenService());
            var request = new RegisterUserRequest
            {
                Name = "Cliente",
                Email = "repetido@example.com",
                Password = "uma-senha-longa-de-teste"
            };
            await authService.RegisterAsync(request);

            await Assert.ThrowsAsync<InvalidOperationException>(() => authService.RegisterAsync(request));
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
