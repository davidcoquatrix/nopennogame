using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;
using NPNG.Domain.Models;

namespace NPNG.Domain.Services;

/// <summary>
/// Service pur du domaine responsable du calcul des scores et du classement des joueurs.
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// Calcule le score total de chaque joueur et détermine leur rang en fonction du type de score du jeu.
    /// </summary>
    public static ImmutableArray<PlayerScore> CalculateLeaderboard(
        ScoreType scoreType,
        IEnumerable<Guid> playerIds,
        IEnumerable<ScoreEntry> entries)
    {
        // 1. Agréger les scores par joueur
        var totalScores = playerIds.ToDictionary(id => id, id => 0);

        foreach (var entry in entries)
        {
            if (totalScores.ContainsKey(entry.PlayerId))
            {
                totalScores[entry.PlayerId] += entry.Value;
            }
        }

        // 2. Trier les joueurs en fonction de la règle du jeu (Score le plus haut ou le plus bas gagne)
        var sortedScores = scoreType switch
        {
            ScoreType.Cumulative => totalScores.OrderByDescending(kvp => kvp.Value).ToList(),
            ScoreType.CumulativeLower => totalScores.OrderBy(kvp => kvp.Value).ToList(),
            _ => throw new NotImplementedException($"Le type de score {scoreType} n'est pas encore implémenté.")
        };

        // 3. Assigner les rangs (gérer les égalités)
        var leaderboard = new List<PlayerScore>();
        int currentRank = 1;
        int displayedRank = 1;
        int? previousScore = null;

        foreach (var kvp in sortedScores)
        {
            if (previousScore.HasValue && previousScore != kvp.Value)
            {
                displayedRank = currentRank;
            }

            leaderboard.Add(new PlayerScore(kvp.Key, kvp.Value, displayedRank));
            
            previousScore = kvp.Value;
            currentRank++;
        }

        return [.. leaderboard];
    }
}
