using HarmonyPlugins.Api.Application.DTOs.Games;

namespace HarmonyPlugins.Api.Application.Interfaces;

public interface IGameService
{
    Task<List<GameResponse>> ListAsync(bool onlyActive);

    Task<GameResponse> CreateAsync(GameRequest request);

    Task<GameResponse> UpdateAsync(Guid id, GameRequest request);
}
