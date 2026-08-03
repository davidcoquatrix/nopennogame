using NPNG.Domain.Entities;

namespace NPNG.Application.Interfaces;

public interface IPlayerProfileRepository
{
    Task<IEnumerable<Player>> GetFavoritePlayersAsync();
    Task AddFavoritePlayerAsync(Player player);
    Task UpdateFavoritePlayerAsync(Player player);
    Task RemoveFavoritePlayerAsync(Guid playerId);
}
