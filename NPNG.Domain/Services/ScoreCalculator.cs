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

        // 2. Trier les joueurs en fonction de la règle du jeu (Score le plus haut ou le plus bas gagne),
        // ou, pour PhaseProgression, par phase atteinte d'abord (le score ne départage qu'à égalité).
        if (scoreType == ScoreType.PhaseProgression)
        {
            var phaseCounts = totalScores.Keys.ToDictionary(
                playerId => playerId,
                playerId => PhaseProgressCalculator.GetCompletedPhaseCount(playerId, entries));

            var sortedByPhase = totalScores
                .OrderByDescending(kvp => phaseCounts[kvp.Key])
                .ThenBy(kvp => kvp.Value)
                .ToList();

            return AssignRanks(
                sortedByPhase,
                kvp => (phaseCounts[kvp.Key], kvp.Value),
                (kvp, rank) => new PlayerScore(kvp.Key, kvp.Value, rank));
        }

        var sortedScores = scoreType switch
        {
            ScoreType.Cumulative or ScoreType.Structured => totalScores.OrderByDescending(kvp => kvp.Value).ToList(),
            ScoreType.CumulativeLower => totalScores.OrderBy(kvp => kvp.Value).ToList(),
            _ => throw new NotImplementedException($"Le type de score {scoreType} n'est pas encore implémenté.")
        };

        // 3. Assigner les rangs (gérer les égalités)
        return AssignRanks(sortedScores, kvp => kvp.Value, (kvp, rank) => new PlayerScore(kvp.Key, kvp.Value, rank));
    }

    /// <summary>
    /// Regroupe un classement individuel déjà calculé par équipe (chaque coéquipier partage le même
    /// score total par construction, puisque la même valeur est enregistrée chez tous les membres).
    /// Le rang est recalculé au niveau de l'équipe, pas du joueur.
    /// </summary>
    public static ImmutableArray<TeamScore> GroupIntoTeams(
        ImmutableArray<PlayerScore> leaderboard,
        ImmutableArray<SessionPlayer> players,
        ImmutableArray<Team> teams)
    {
        var playersById = players.ToDictionary(p => p.PlayerId);

        var groupedTeams = leaderboard
            .GroupBy(ps => playersById[ps.PlayerId].TeamId
                ?? throw new InvalidOperationException("Tous les joueurs doivent appartenir à une équipe."))
            .Select(g => new
            {
                TeamId = g.Key,
                TotalScore = g.First().TotalScore,
                MemberPlayerIds = g.Select(ps => ps.PlayerId).ToImmutableArray()
            })
            .ToList();

        return AssignRanks(
            groupedTeams,
            t => t.TotalScore,
            (t, rank) => new TeamScore(
                t.TeamId,
                TeamNameFormatter.GetDisplayName(t.TeamId, teams, players),
                TeamEmojiFormatter.GetDisplayEmoji(t.TeamId, teams),
                t.MemberPlayerIds,
                t.TotalScore,
                rank));
    }

    /// <summary>
    /// Assigne un rang à chaque élément d'une liste déjà triée du meilleur au moins bon,
    /// en attribuant le même rang aux ex-aequo et en sautant les rangs suivants. La clé d'égalité
    /// (<typeparamref name="TKey"/>) peut être un score simple (<c>int</c>) ou une clé composite
    /// (ex: <c>(int Phase, int Score)</c> pour PhaseProgression, où l'égalité doit porter sur les deux).
    /// </summary>
    private static ImmutableArray<TResult> AssignRanks<TItem, TKey, TResult>(
        List<TItem> orderedBestToWorst,
        Func<TItem, TKey> rankKeySelector,
        Func<TItem, int, TResult> resultSelector)
        where TKey : struct, IEquatable<TKey>
    {
        var results = new List<TResult>();
        int currentRank = 1;
        int displayedRank = 1;
        TKey? previousKey = null;

        foreach (var item in orderedBestToWorst)
        {
            var key = rankKeySelector(item);
            if (previousKey.HasValue && !previousKey.Value.Equals(key))
            {
                displayedRank = currentRank;
            }

            results.Add(resultSelector(item, displayedRank));

            previousKey = key;
            currentRank++;
        }

        return [.. results];
    }
}
