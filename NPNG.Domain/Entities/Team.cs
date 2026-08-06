namespace NPNG.Domain.Entities;

/// <summary>
/// Une équipe formée pour la session en cours. Les <see cref="SessionPlayer"/> qui en font
/// partie ne portent qu'une référence (<see cref="SessionPlayer.TeamId"/>) — le nom et l'emoji
/// personnalisés vivent ici, en un seul exemplaire, plutôt que dupliqués sur chaque membre.
/// </summary>
public record Team(
    Guid TeamId,
    string? CustomName = null,
    string? CustomEmoji = null);
