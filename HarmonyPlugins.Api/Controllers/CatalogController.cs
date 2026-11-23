using HarmonyPlugins.Api.Application.DTOs.Catalog;
using HarmonyPlugins.Api.Application.DTOs.Categories;
using HarmonyPlugins.Api.Application.DTOs.Games;
using HarmonyPlugins.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers;

// UC03: o catálogo também é aberto a visitantes.
[ApiController]
[AllowAnonymous]
[Route("api/catalog")]
public class CatalogController(
    ICatalogService catalogService,
    IGameService gameService,
    ICategoryService categoryService) : ControllerBase
{
    [HttpGet("plugins")]
    public async Task<ActionResult<PagedResponse<CatalogPluginResponse>>> Search([FromQuery] CatalogQuery query) =>
        await catalogService.SearchAsync(query);

    [HttpGet("plugins/{slug}")]
    public async Task<ActionResult<CatalogPluginDetailResponse>> Get(string slug) =>
        await catalogService.GetBySlugAsync(slug);

    // Opções dos filtros: só jogos e categorias ativos.
    [HttpGet("games")]
    public async Task<ActionResult<List<GameResponse>>> Games() =>
        await gameService.ListAsync(onlyActive: true);

    [HttpGet("categories")]
    public async Task<ActionResult<List<CategoryResponse>>> Categories() =>
        await categoryService.ListAsync(onlyActive: true);
}
