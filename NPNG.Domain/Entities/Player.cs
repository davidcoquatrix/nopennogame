namespace NPNG.Domain.Entities;

/// <summary>
/// Représente un joueur (profil global, indépendant d'une partie).
/// </summary>
public record Player(
    Guid Id,
    string Name,
    string DefaultEmoji);
