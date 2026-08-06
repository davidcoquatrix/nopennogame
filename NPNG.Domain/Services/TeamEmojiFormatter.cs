using System.Collections.Immutable;
using NPNG.Domain.Entities;

namespace NPNG.Domain.Services;

/// <summary>
/// Calcule l'emoji affiché d'une équipe : son emoji personnalisé s'il a été défini,
/// sinon l'emoji par défaut "🤝".
/// </summary>
public static class TeamEmojiFormatter
{
    public const string DefaultEmoji = "🤝";

    public static string GetDisplayEmoji(Guid teamId, ImmutableArray<Team> teams)
    {
        var customEmoji = teams.FirstOrDefault(t => t.TeamId == teamId)?.CustomEmoji;

        return !string.IsNullOrWhiteSpace(customEmoji) ? customEmoji : DefaultEmoji;
    }
}
