using HarmonyPlugins.Api.Application.DTOs.Catalog;

namespace HarmonyPlugins.Api.Application.Interfaces;

public interface ICatalogService
{
    Task<PagedResponse<CatalogPluginResponse>> SearchAsync(CatalogQuery query);

    Task<CatalogPluginDetailResponse> GetBySlugAsync(string slug);
}
