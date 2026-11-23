using HarmonyPlugins.Api.Application.Common;
using HarmonyPlugins.Api.Application.DTOs.Catalog;
using HarmonyPlugins.Api.Application.Services;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Domain.Exceptions;

namespace HarmonyPlgins.Tests;

public class CatalogServiceTests
{
    [Fact]
    public async Task SearchAsync_ListsOnlyPublishedPluginsMatchingFilters()
    {
        await using var dbContext = TestDb.Create();
        var (game, category) = await TestDb.SeedGameAndCategoryAsync(dbContext);
        dbContext.Plugins.AddRange(
            TestDb.NewPlugin(game, category, "Loja Pago", PluginStatus.Published),
            TestDb.NewPlugin(game, category, "Loja Gratis", PluginStatus.Published, PluginType.Free, 0),
            TestDb.NewPlugin(game, category, "Loja Rascunho", PluginStatus.Draft),
            TestDb.NewPlugin(game, category, "Loja Suspensa", PluginStatus.Inactive));
        await dbContext.SaveChangesAsync();
        var service = new CatalogService(dbContext);

        var all = await service.SearchAsync(new CatalogQuery { Search = "Loja" });
        var free = await service.SearchAsync(new CatalogQuery { Type = PluginType.Free });
        var expensive = await service.SearchAsync(new CatalogQuery { MinPrice = 10 });

        Assert.Equal(["Loja Gratis", "Loja Pago"], all.Items.Select(p => p.Name));
        Assert.Equal("Loja Gratis", Assert.Single(free.Items).Name);
        Assert.Equal("Loja Pago", Assert.Single(expensive.Items).Name);
    }

    [Fact]
    public async Task GetBySlugAsync_WithDraftPlugin_ThrowsNotFound()
    {
        await using var dbContext = TestDb.Create();
        var (game, category) = await TestDb.SeedGameAndCategoryAsync(dbContext);
        dbContext.Plugins.Add(TestDb.NewPlugin(game, category, "Rascunho", PluginStatus.Draft));
        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => new CatalogService(dbContext).GetBySlugAsync("rascunho"));
    }

    [Theory]
    [InlineData("Economia & Lojas", "economia-lojas")]
    [InlineData("  Administração  ", "administracao")]
    [InlineData("!!!", "")]
    public void Slug_From_NormalizesText(string text, string expected)
    {
        Assert.Equal(expected, Slug.From(text));
    }
}
