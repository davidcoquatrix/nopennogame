using NPNG.Domain.Entities;

namespace NPNG.Application.Interfaces;

public interface IPlayerProfileRepository
{
    Task<Player?> GetDefaultProfileAsync();
    Task SaveDefaultProfileAsync(Player profile);
    
    Task<IEnumerable<Player>> GetFavoritePlayersAsync();
    Task AddFavoritePlayerAsync(Player player);
    Task RemoveFavoritePlayerAsync(Guid playerId);
}