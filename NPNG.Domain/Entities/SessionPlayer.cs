namespace NPNG.Domain.Entities;

/// <summary>
/// Représente la participation d'un joueur à une session de jeu spécifique.
/// Contient les attributs visuels et l'ordre de jeu pour cette session uniquement.
/// </summary>
public record SessionPlayer(
    Guid PlayerId,
    int DisplayOrder,
    string Color);
