using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.DTOs.Users;

namespace HarmonyPlugins.Api.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterAsync(RegisterUserRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> RefreshAsync(string refreshToken);

        Task LogoutAsync(string refreshToken);
    }
}
