using HarmonyPlugins.Api.Application.DTOs.Plugins;

namespace HarmonyPlugins.Api.Application.Interfaces;

public interface IPluginService
{
    Task<List<PluginResponse>> ListAsync();

    Task<PluginResponse> GetAsync(Guid id);

    Task<PluginResponse> CreateAsync(PluginRequest request);

    Task<PluginResponse> UpdateAsync(Guid id, PluginRequest request);

    Task<PluginResponse> PublishAsync(Guid id);

    Task<PluginResponse> UnpublishAsync(Guid id);

    Task<PluginVersionResponse> AddVersionAsync(Guid pluginId, PluginVersionRequest request, CancellationToken cancellationToken = default);

    Task<PluginVersionResponse> SetCurrentVersionAsync(Guid pluginId, Guid versionId);
}
