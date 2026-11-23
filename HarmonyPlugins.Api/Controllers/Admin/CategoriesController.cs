using HarmonyPlugins.Api.Application.DTOs.Categories;
using HarmonyPlugins.Api.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> List() =>
        await categoryService.ListAsync(onlyActive: false);

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CategoryRequest request) =>
        StatusCode(StatusCodes.Status201Created, await categoryService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, [FromBody] CategoryRequest request) =>
        await categoryService.UpdateAsync(id, request);
}
