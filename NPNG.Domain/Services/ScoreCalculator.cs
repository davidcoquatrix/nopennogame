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
    /// en attribuant le même rang aux ex-aequo et en sautant les rangs suivants.
    /// </summary>
    private static ImmutableArray<TResult> AssignRanks<TItem, TResult>(
        List<TItem> orderedBestToWorst,
        Func<TItem, int> scoreSelector,
        Func<TItem, int, TResult> resultSelector)
    {
        var results = new List<TResult>();
        int currentRank = 1;
        int displayedRank = 1;
        int? previousScore = null;

        foreach (var item in orderedBestToWorst)
        {
            var score = scoreSelector(item);
            if (previousScore.HasValue && previousScore != score)
            {
                displayedRank = currentRank;
            }

            results.Add(resultSelector(item, displayedRank));

            previousScore = score;
            currentRank++;
        }

        return [.. results];
    }
}
