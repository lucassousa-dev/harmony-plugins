using HarmonyPlugins.Api.Application.DTOs.Auth;
using HarmonyPlugins.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers
{
    [ApiController]
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
            try
            {
                var userCreated = await _authService.RegisterAsync(request);
                return StatusCode(201, userCreated);
            }
            catch (InvalidOperationException ex) {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
