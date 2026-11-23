using HarmonyPlugins.Api.Application.DTOs.Games;
using HarmonyPlugins.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/games")]
public class GamesController(IGameService gameService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GameResponse>>> List() =>
        await gameService.ListAsync(onlyActive: false);

    [HttpPost]
    public async Task<ActionResult<GameResponse>> Create([FromBody] GameRequest request) =>
        StatusCode(StatusCodes.Status201Created, await gameService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GameResponse>> Update(Guid id, [FromBody] GameRequest request) =>
        await gameService.UpdateAsync(id, request);
}
