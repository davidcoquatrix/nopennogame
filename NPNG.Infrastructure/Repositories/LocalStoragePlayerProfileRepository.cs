using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

public class LocalStoragePlayerProfileRepository(IJSRuntime jsRuntime) : IPlayerProfileRepository
{
    private const string FavoritesKey = "npng_favorite_players";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<IEnumerable<Player>> GetFavoritePlayersAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", FavoritesKey);

        if (string.IsNullOrEmpty(json))
        {
            return Enumerable.Empty<Player>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Player>>(json, _jsonOptions) ?? new List<Player>();
        }
        catch (JsonException)
        {
            return Enumerable.Empty<Player>();
        }
    }

    public async Task AddFavoritePlayerAsync(Player player)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();

        // Prevent duplicates by name
        if (!favorites.Any(f => f.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase)))
        {
            favorites.Add(player);
            var json = JsonSerializer.Serialize(favorites, _jsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", FavoritesKey, json);
        }
    }

    public async Task UpdateFavoritePlayerAsync(Player player)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();
        var index = favorites.FindIndex(f => f.Id == player.Id);

        if (index >= 0)
        {
            favorites[index] = player;
            var json = JsonSerializer.Serialize(favorites, _jsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", FavoritesKey, json);
        }
    }

    public async Task RemoveFavoritePlayerAsync(Guid playerId)
    {
        var favorites = (await GetFavoritePlayersAsync()).ToList();
        var itemToRemove = favorites.FirstOrDefault(f => f.Id == playerId);

        if (itemToRemove != null)
        {
            favorites.Remove(itemToRemove);
            var json = JsonSerializer.Serialize(favorites, _jsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", FavoritesKey, json);
        }
    }
}
