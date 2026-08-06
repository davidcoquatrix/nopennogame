using System.Collections.Immutable;
using NPNG.Domain.Entities;

namespace NPNG.Domain.Services;

/// <summary>
/// Calcule le nom affiché d'une équipe : son nom personnalisé s'il a été défini,
/// sinon un nom généré à partir des membres (ex: "David & Marion").
/// </summary>
public static class TeamNameFormatter
{
    public static string GetDisplayName(Guid teamId, ImmutableArray<Team> teams, IEnumerable<SessionPlayer> allPlayers)
    {
        var customName = teams.FirstOrDefault(t => t.TeamId == teamId)?.CustomName;
        if (!string.IsNullOrWhiteSpace(customName))
        {
            return customName;
        }

        var members = allPlayers
            .Where(p => p.TeamId == teamId)
            .OrderBy(p => p.DisplayOrder);

        return string.Join(" & ", members.Select(p => p.Name));
    }
}
