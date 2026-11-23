using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HarmonyPlugins.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            var userCreated = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, userCreated);
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return Ok(await _authService.LoginAsync(request));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            return Ok(await _authService.RefreshAsync(request.RefreshToken));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return NoContent();
        }
    }
}
