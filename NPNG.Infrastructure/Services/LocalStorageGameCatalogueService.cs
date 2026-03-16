using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Application.Models;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Infrastructure.Services;

/// <summary>
/// Implémentation du catalogue de jeux utilisant le LocalStorage pour les jeux personnalisés.
/// Retourne les jeux de base + les jeux personnalisés sauvegardés par l'utilisateur.
/// </summary>
public class LocalStorageGameCatalogueService(IJSRuntime jsRuntime) : IGameCatalogueService
{
    private const string CustomGamesKey = "npng_custom_games";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly List<GameCatalogueItem> _baseGames = new()
    {
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "🃏", "Skyjo", "Cartes −2 à 12. Chaque joueur révèle ses cartes. Le total le plus bas gagne. La partie s'arrête dès qu'un joueur atteint 100 pts.", "Le + bas gagne", "#7C9EBF", ScoreType.CumulativeLower, new GameRules(TargetScore: 100)),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "🂡", "Rami", "Défausser toutes ses cartes en formant des combinaisons. Le joueur avec le score cumulé le plus élevé en fin de manche gagne.", "Le + haut gagne", "#4ADE80", ScoreType.Cumulative),
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "🎴", "Uno", "Se débarrasser de toutes ses cartes en premier. Les cartes restantes en main sont comptées comme points de pénalité.", "Le + bas gagne", "#F87171", ScoreType.CumulativeLower),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "🏛️", "Accropolis", "Construire une cité en posant des tuiles par catégories. Chaque catégorie a son propre système de points. Le plus haut total gagne.", "Structuré", "var(--accent)", ScoreType.Structured),
        new(Guid.Parse("55555555-5555-5555-5555-555555555555"), "⚙️", "Jeu personnalisé", "Définis ton propre nom de jeu, la règle de victoire (+ haut ou + bas) et les limites optionnelles de score ou de tours.", "Personnalisé", "", ScoreType.Cumulative, null, true)
    };

    public async Task<IEnumerable<GameCatalogueItem>> GetAvailableGamesAsync()
    {
        var customGames = await GetCustomGamesAsync();
        return _baseGames.Concat(customGames);
    }

    public async Task SaveCustomGameAsync(GameTemplate template, string color)
    {
        var customGames = (await GetCustomGamesAsync()).ToList();

        // Convert GameTemplate to GameCatalogueItem
        var description = template.Rules != null 
            ? $"Règles personnalisées. Type de score : {template.ScoreType}"
            : "Jeu personnalisé";

        var winRule = template.ScoreType == ScoreType.CumulativeLower ? "Le + bas gagne" : "Le + haut gagne";

        var item = new GameCatalogueItem(
            template.Id, 
            "📌", // Emoji par défaut pour un jeu perso
            template.Name, 
            description, 
            winRule, 
            color, 
            template.ScoreType, 
            template.Rules, 
            true); // Set to true for custom games

        // Update if exists, else add
        var existingIndex = customGames.FindIndex(g => g.Id == template.Id);
        if (existingIndex >= 0)
        {
            customGames[existingIndex] = item;
        }
        else
        {
            customGames.Add(item);
        }

        var json = JsonSerializer.Serialize(customGames, _jsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", CustomGamesKey, json);
    }

    public async Task DeleteCustomGameAsync(Guid id)
    {
        var customGames = (await GetCustomGamesAsync()).ToList();
        var itemToRemove = customGames.FirstOrDefault(g => g.Id == id);

        if (itemToRemove != null)
        {
            customGames.Remove(itemToRemove);
            var json = JsonSerializer.Serialize(customGames, _jsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", CustomGamesKey, json);
        }
    }

    private async Task<IEnumerable<GameCatalogueItem>> GetCustomGamesAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", CustomGamesKey);

        if (string.IsNullOrEmpty(json))
        {
            return Enumerable.Empty<GameCatalogueItem>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<GameCatalogueItem>>(json, _jsonOptions) ?? new List<GameCatalogueItem>();
            // Forcer IsCustom = true pour les parties sauvegardées avant la correction du bug
            return items.Select(i => i with { IsCustom = true });
        }
        catch (JsonException)
        {
            return Enumerable.Empty<GameCatalogueItem>();
        }
    }
}
