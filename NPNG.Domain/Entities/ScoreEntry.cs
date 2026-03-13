namespace NPNG.Domain.Entities;

/// <summary>
/// Représente le score brut saisi pour un joueur lors d'une manche spécifique.
/// </summary>
public record ScoreEntry(
    Guid Id,
    Guid SessionId,
    Guid PlayerId,
    int Round,
    int Value);
