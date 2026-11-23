using HarmonyPlugins.Api.Domain.Entities;

namespace HarmonyPlugins.Api.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
