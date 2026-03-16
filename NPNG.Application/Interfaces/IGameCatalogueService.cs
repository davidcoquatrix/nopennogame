using NPNG.Application.Models;

namespace NPNG.Application.Interfaces;

/// <summary>
/// Service de récupération du catalogue de jeux.
/// </summary>
public interface IGameCatalogueService
{
    Task<IEnumerable<GameCatalogueItem>> GetAvailableGamesAsync();
}
