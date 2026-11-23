using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HarmonyPlugins.Api.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("A chave JWT não foi configurada.");
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("O emissor JWT não foi configurado.");
            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("A audiência JWT não foi configurada.");
            var expirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

            if (Encoding.UTF8.GetByteCount(key) < 32)
                throw new InvalidOperationException("A chave JWT deve ter pelo menos 32 bytes.");

            if (expirationMinutes <= 0)
                throw new InvalidOperationException("O tempo de expiração do JWT deve ser maior que zero.");

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
