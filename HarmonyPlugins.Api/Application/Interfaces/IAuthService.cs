using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.DTOs.Users;

namespace HarmonyPlugins.Api.Application.Interfaces
{
    public interface IAuthService
    {
        public Task<UserResponse> RegisterAsync(RegisterUserRequest request);
    }
}
