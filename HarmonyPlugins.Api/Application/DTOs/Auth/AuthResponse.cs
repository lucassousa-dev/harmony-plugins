using HarmonyPlugins.Api.Application.DTOs.Users;

namespace HarmonyPlugins.Api.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public UserResponse User { get; set; }
    }
}
