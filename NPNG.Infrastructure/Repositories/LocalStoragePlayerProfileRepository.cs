using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

public class LocalStoragePlayerProfileRepository(IJSRuntime jsRuntime) : IPlayerProfileRepository
{
    private const string ProfileKey = "npng_default_profile";
    private const string FavoritesKey = "npng_favorite_players";
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<Player?> GetDefaultProfileAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", ProfileKey);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Player>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task SaveDefaultProfileAsync(Player profile)
    {
        var json = JsonSerializer.Serialize(profile, _jsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", ProfileKey, json);
    }

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
