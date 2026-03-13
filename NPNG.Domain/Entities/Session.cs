using System.Collections.Immutable;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Représente une partie (session) en cours, terminée ou abandonnée.
/// Utilise des collections immuables pour respecter nos standards de qualité.
/// </summary>
public record Session(
    Guid Id,
    Guid TemplateId,
    DateTime StartedAt,
    ImmutableArray<SessionPlayer> Players,
    DateTime? EndedAt = null,
    SessionStatus Status = SessionStatus.Active);
