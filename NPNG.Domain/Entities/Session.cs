using System.Collections.Immutable;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Représente une partie (session) en cours, terminée ou abandonnée.
/// Utilise des collections immuables pour respecter nos standards de qualité (DDD).
/// </summary>
public record Session(
    Guid Id,
    GameTemplate Template,
    DateTime StartedAt,
    ImmutableArray<SessionPlayer> Players,
    ImmutableArray<ScoreEntry> Scores,
    int CurrentRound = 1,
    DateTime? EndedAt = null,
    SessionStatus Status = SessionStatus.Active)
{
    public static Session Create(GameTemplate template)
    {
        return new Session(
            Guid.NewGuid(),
            template,
            DateTime.UtcNow,
            ImmutableArray<SessionPlayer>.Empty,
            ImmutableArray<ScoreEntry>.Empty);
    }
}
