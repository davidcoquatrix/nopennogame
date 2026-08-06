using System.Collections.Immutable;

namespace NPNG.Domain.Models;

/// <summary>
/// Représente le score total calculé pour une équipe et son classement actuel.
/// </summary>
public record TeamScore(
    Guid TeamId,
    string DisplayName,
    string DisplayEmoji,
    ImmutableArray<Guid> MemberPlayerIds,
    int TotalScore,
    int Rank);
