using NPNG.Application.Interfaces;
using NPNG.Application.Models;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Application.Services;

/// <summary>
/// Implémentation en mémoire du catalogue de jeux (Phase 1).
/// Contient la liste des jeux disponibles dans l'application.
/// </summary>
public class InMemoryGameCatalogueService : IGameCatalogueService
{
    private readonly List<GameCatalogueItem> _games = new()
    {
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "🃏", "Skyjo", "Cartes −2 à 12. Chaque joueur révèle ses cartes. Le total le plus bas gagne. La partie s'arrête dès qu'un joueur atteint 100 pts.", "Le + bas gagne", "#7C9EBF", ScoreType.CumulativeLower, new GameRules(TargetScore: 100)),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "🂡", "Rami", "Défausser toutes ses cartes en formant des combinaisons. Le joueur avec le score cumulé le plus élevé en fin de manche gagne.", "Le + haut gagne", "#4ADE80", ScoreType.Cumulative),
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "🎴", "Uno", "Se débarrasser de toutes ses cartes en premier. Les cartes restantes en main sont comptées comme points de pénalité.", "Le + bas gagne", "#F87171", ScoreType.CumulativeLower),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "🏛️", "Accropolis", "Construire une cité en posant des tuiles par catégories. Chaque catégorie a son propre système de points. Le plus haut total gagne.", "Structuré", "var(--accent)", ScoreType.Structured),
        new(Guid.Parse("55555555-5555-5555-5555-555555555555"), "⚙️", "Jeu personnalisé", "Définis ton propre nom de jeu, la règle de victoire (+ haut ou + bas) et les limites optionnelles de score ou de tours.", "Personnalisé", "", ScoreType.Cumulative, null, true)
    };

    public Task<IEnumerable<GameCatalogueItem>> GetAvailableGamesAsync()
    {
        return Task.FromResult<IEnumerable<GameCatalogueItem>>(_games);
    }
}
