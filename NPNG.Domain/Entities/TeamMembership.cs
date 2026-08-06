namespace NPNG.Domain.Entities;

/// <summary>
/// Appartenance d'un <see cref="SessionPlayer"/> à une équipe pour la session en cours.
/// Regroupe des champs qui changent toujours ensemble (créés/effacés en bloc à chaque
/// (re)formation d'équipe) plutôt que de les laisser à plat sur SessionPlayer.
/// </summary>
public record TeamMembership(
    Guid TeamId,
    string? CustomName = null,
    string? CustomEmoji = null);
