using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace HarmonyPlgins.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void GenerateToken_WithValidConfiguration_CreatesExpectedClaims()
        {
            var configuration = CreateConfiguration("harmony-plugins-test-key-with-at-least-32-bytes");
            var tokenService = new TokenService(configuration);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Administrador Teste",
                Email = "admin@example.com",
                PasswordHash = "hash",
                Role = UserRole.Admin
            };

            var token = tokenService.GenerateToken(user);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            Assert.Equal("HarmonyPlugins.Api", jwt.Issuer);
            Assert.Contains("HarmonyPlugins.Client", jwt.Audiences);
            Assert.Equal(user.Id.ToString(), jwt.Subject);
            Assert.Contains(jwt.Claims, claim =>
                claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
            Assert.Contains(jwt.Claims, claim =>
                claim.Type == ClaimTypes.Role && claim.Value == "Admin");
            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_WithShortKey_ThrowsInvalidOperationException()
        {
            var configuration = CreateConfiguration("short-key");
            var tokenService = new TokenService(configuration);
            var user = new User
            {
                Name = "Cliente Teste",
                Email = "cliente@example.com",
                PasswordHash = "hash"
            };

            var exception = Assert.Throws<InvalidOperationException>(() =>
                tokenService.GenerateToken(user));

            Assert.Equal("A chave JWT deve ter pelo menos 32 bytes.", exception.Message);
        }

        private static IConfiguration CreateConfiguration(string key)
        {
            var values = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = key,
                ["Jwt:Issuer"] = "HarmonyPlugins.Api",
                ["Jwt:Audience"] = "HarmonyPlugins.Client",
                ["Jwt:ExpirationMinutes"] = "60"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}
