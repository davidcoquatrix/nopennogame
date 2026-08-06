using NPNG.Domain.Entities;

namespace NPNG.Domain.Services;

/// <summary>
/// Calcule l'emoji affiché d'une équipe : son emoji personnalisé s'il a été défini,
/// sinon l'emoji par défaut "🤝".
/// </summary>
public static class TeamEmojiFormatter
{
    public const string DefaultEmoji = "🤝";

    public static string GetDisplayEmoji(Guid teamId, IEnumerable<SessionPlayer> allPlayers)
    {
        var customEmoji = allPlayers
            .Where(p => p.Team?.TeamId == teamId)
            .OrderBy(p => p.DisplayOrder)
            .FirstOrDefault()
            ?.Team?.CustomEmoji;

        return !string.IsNullOrWhiteSpace(customEmoji) ? customEmoji : DefaultEmoji;
    }
}
