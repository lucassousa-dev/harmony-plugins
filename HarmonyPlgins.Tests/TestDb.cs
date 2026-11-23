using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Enums;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HarmonyPlgins.Tests;

internal static class TestDb
{
    // O InMemory não tem transações; o aviso é ignorado para testar o código que as usa.
    public static AppDbContext Create() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    public static async Task<(Game Game, PluginCategory Category)> SeedGameAndCategoryAsync(AppDbContext dbContext)
    {
        var game = new Game { Name = "Minecraft" };
        var category = new PluginCategory { Name = "Economia", Slug = "economia" };
        dbContext.AddRange(game, category);
        await dbContext.SaveChangesAsync();
        return (game, category);
    }

    public static Plugin NewPlugin(Game game, PluginCategory category, string name, PluginStatus status,
        PluginType type = PluginType.Paid, decimal price = 20m) => new()
    {
        Name = name,
        Slug = name.ToLowerInvariant().Replace(' ', '-'),
        ShortDescription = "Resumo",
        FullDescription = "Descrição",
        GameId = game.Id,
        CategoryId = category.Id,
        Type = type,
        Price = price,
        Status = status
    };
}
