using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "Harmony Plugins API",
            timestamp = DateTime.UtcNow
        });
    }
}