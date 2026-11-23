using HarmonyPlugins.Api.Application.DTOs.Plugins;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HarmonyPlugins.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/plugins")]
public class PluginsController(IPluginService pluginService) : ControllerBase
{
    // Folga para os campos do formulário além do arquivo de até 100 MB.
    private const long MaxUploadRequestSize = PluginService.MaxFileSize + 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<List<PluginResponse>>> List() =>
        await pluginService.ListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PluginResponse>> Get(Guid id) =>
        await pluginService.GetAsync(id);

    [HttpPost]
    public async Task<ActionResult<PluginResponse>> Create([FromBody] PluginRequest request) =>
        StatusCode(StatusCodes.Status201Created, await pluginService.CreateAsync(request));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PluginResponse>> Update(Guid id, [FromBody] PluginRequest request) =>
        await pluginService.UpdateAsync(id, request);

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<PluginResponse>> Publish(Guid id) =>
        await pluginService.PublishAsync(id);

    [HttpPost("{id:guid}/unpublish")]
    public async Task<ActionResult<PluginResponse>> Unpublish(Guid id) =>
        await pluginService.UnpublishAsync(id);

    [HttpPost("{id:guid}/versions")]
    [RequestSizeLimit(MaxUploadRequestSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestSize)]
    public async Task<ActionResult<PluginVersionResponse>> AddVersion(
        Guid id, [FromForm] PluginVersionRequest request, CancellationToken cancellationToken) =>
        StatusCode(StatusCodes.Status201Created, await pluginService.AddVersionAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/versions/{versionId:guid}/current")]
    public async Task<ActionResult<PluginVersionResponse>> SetCurrentVersion(Guid id, Guid versionId) =>
        await pluginService.SetCurrentVersionAsync(id, versionId);
}
