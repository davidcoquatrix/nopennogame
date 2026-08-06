using NPNG.Domain.Entities;

namespace NPNG.Domain.Services;

/// <summary>
/// Calcule le nom affiché d'une équipe : son nom personnalisé s'il a été défini,
/// sinon un nom généré à partir des membres (ex: "David & Marion").
/// </summary>
public static class TeamNameFormatter
{
    public static string GetDisplayName(Guid teamId, IEnumerable<SessionPlayer> allPlayers)
    {
        var members = allPlayers
            .Where(p => p.Team?.TeamId == teamId)
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        var customName = members.FirstOrDefault()?.Team?.CustomName;

        return !string.IsNullOrWhiteSpace(customName)
            ? customName
            : string.Join(" & ", members.Select(p => p.Name));
    }
}
