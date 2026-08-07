using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

public class LocalStoragePlayerProfileRepository(ILocalStorageService localStorage) : IPlayerProfileRepository
{
    private const string FavoritesKey = "npng_favorite_players";

    public async Task<IEnumerable<Player>> GetFavoritePlayersAsync()
    {
        var favorites = await localStorage.GetItemAsync<List<Player>>(FavoritesKey);
        return favorites ?? Enumerable.Empty<Player>();
    }

    public async Task AddFavoritePlayerAsync(Player player)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();

        // Prevent duplicates by name
        if (!favorites.Any(f => f.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
        {
            favorites.Add(player);
            await localStorage.SetItemAsync(FavoritesKey, favorites);
        }
    }

    public async Task UpdateFavoritePlayerAsync(Player player)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();
        var index = favorites.FindIndex(f => f.Id == player.Id);

        if (index >= 0)
        {
            favorites[index] = player;
            await localStorage.SetItemAsync(FavoritesKey, favorites);
        }
    }

    public async Task RemoveFavoritePlayerAsync(Guid playerId)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();
        var itemToRemove = favorites.FirstOrDefault(f => f.Id == playerId);

        if (itemToRemove != null)
        {
            favorites.Remove(itemToRemove);
            await localStorage.SetItemAsync(FavoritesKey, favorites);
        }
    }
}
