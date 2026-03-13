namespace NPNG.Domain.Models;

/// <summary>
/// Représente le score total calculé pour un joueur et son classement actuel.
/// </summary>
public record PlayerScore(
    Guid PlayerId,
    int TotalScore,
    int Rank);
