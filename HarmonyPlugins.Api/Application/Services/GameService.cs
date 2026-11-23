using HarmonyPlugins.Api.Application.DTOs.Games;
using HarmonyPlugins.Api.Application.Interfaces;
using HarmonyPlugins.Api.Domain.Entities;
using HarmonyPlugins.Api.Domain.Exceptions;
using HarmonyPlugins.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HarmonyPlugins.Api.Application.Services;

public class GameService(AppDbContext dbContext, IAuditService auditService) : IGameService
{
    public Task<List<GameResponse>> ListAsync(bool onlyActive) =>
        dbContext.Games.AsNoTracking()
            .Where(g => !onlyActive || g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new GameResponse { Id = g.Id, Name = g.Name, IsActive = g.IsActive })
            .ToListAsync();

    public async Task<GameResponse> CreateAsync(GameRequest request)
    {
        var game = new Game();
        await ApplyAsync(game, request);
        dbContext.Games.Add(game);
        auditService.Record("CRIAR_JOGO", "Game", game.Id);
        await dbContext.SaveChangesAsync();
        return ToResponse(game);
    }

    public async Task<GameResponse> UpdateAsync(Guid id, GameRequest request)
    {
        var game = await dbContext.Games.FindAsync(id) ?? throw new NotFoundException("Jogo não encontrado.");
        await ApplyAsync(game, request);
        auditService.Record("EDITAR_JOGO", "Game", game.Id);
        await dbContext.SaveChangesAsync();
        return ToResponse(game);
    }

    private async Task ApplyAsync(Game game, GameRequest request)
    {
        var name = request.Name.Trim();
        if (await dbContext.Games.AnyAsync(g => g.Name == name && g.Id != game.Id))
            throw new ConflictException("Já existe um jogo com esse nome.");

        game.Name = name;
        game.IsActive = request.IsActive;
    }

    private static GameResponse ToResponse(Game game) =>
        new() { Id = game.Id, Name = game.Name, IsActive = game.IsActive };
}
