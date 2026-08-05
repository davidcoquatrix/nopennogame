using NPNG.Application.Models;
using NPNG.Domain.Entities;

namespace NPNG.Application.Interfaces;

/// <summary>
/// Service de récupération du catalogue de jeux.
/// </summary>
public interface IGameCatalogueService
{
    Task<IEnumerable<GameCatalogueItem>> GetAvailableGamesAsync();
    Task SaveCustomGameAsync(GameTemplate template, string color, string emoji);
    Task DeleteCustomGameAsync(Guid id);
}
