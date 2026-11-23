using HarmonyPlugins.Api.Application.DTOs.Catalog;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services;

// UC03: só plugins PUBLICADOS aparecem; rascunhos e suspensos ficam de fora (RN10).
public class CatalogService(AppDbContext dbContext) : ICatalogService
{
    public async Task<PagedResponse<CatalogPluginResponse>> SearchAsync(CatalogQuery query)
    {
        var plugins = dbContext.Plugins.AsNoTracking().Where(p => p.Status == PluginStatus.Published);

        if (!string.IsNullOrWhiteSpace(query.Search))
            plugins = plugins.Where(p => p.Name.Contains(query.Search.Trim()));
        if (query.GameId is not null)
            plugins = plugins.Where(p => p.GameId == query.GameId);
        if (query.CategoryId is not null)
            plugins = plugins.Where(p => p.CategoryId == query.CategoryId);
        if (query.Type is not null)
            plugins = plugins.Where(p => p.Type == query.Type);
        if (query.MinPrice is not null)
            plugins = plugins.Where(p => p.Price >= query.MinPrice);
        if (query.MaxPrice is not null)
            plugins = plugins.Where(p => p.Price <= query.MaxPrice);

        var total = await plugins.CountAsync();
        var items = await plugins
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new CatalogPluginResponse
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                ShortDescription = p.ShortDescription,
                Type = p.Type,
                Price = p.Price,
                BannerUrl = p.BannerUrl,
                GameName = p.Game!.Name,
                CategoryName = p.Category!.Name,
                CurrentVersion = p.Versions
                    .Where(v => v.IsCurrent && v.Status == PluginVersionStatus.Active)
                    .Select(v => v.VersionNumber)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new PagedResponse<CatalogPluginResponse>
        {
            Items = items,
            Total = total,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<CatalogPluginDetailResponse> GetBySlugAsync(string slug)
    {
        // UC03 6a: suspenso, rascunho ou inexistente respondem da mesma forma.
        var plugin = await dbContext.Plugins.AsNoTracking()
            .Include(p => p.Game)
            .Include(p => p.Category)
            .Include(p => p.Versions.Where(v => v.Status == PluginVersionStatus.Active))
            .SingleOrDefaultAsync(p => p.Slug == slug && p.Status == PluginStatus.Published)
            ?? throw new NotFoundException("Plugin indisponível.");

        var current = plugin.Versions.FirstOrDefault(v => v.IsCurrent);

        return new CatalogPluginDetailResponse
        {
            Id = plugin.Id,
            Name = plugin.Name,
            Slug = plugin.Slug,
            ShortDescription = plugin.ShortDescription,
            FullDescription = plugin.FullDescription,
            Type = plugin.Type,
            Price = plugin.Price,
            BannerUrl = plugin.BannerUrl,
            GameName = plugin.Game!.Name,
            CategoryName = plugin.Category!.Name,
            CurrentVersion = current?.VersionNumber,
            Compatibility = current?.Compatibility,
            Versions = plugin.Versions
                .OrderByDescending(v => v.PublishedAt)
                .Select(PluginService.ToResponse)
                .ToList()
        };
    }
}
