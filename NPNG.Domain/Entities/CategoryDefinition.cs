using NPNG.Domain.Enums;

namespace NPNG.Domain.Entities;

/// <summary>
/// Décrit une catégorie de score structuré : son identifiant stable (<see cref="Key"/>, utilisé pour
/// indexer <see cref="StructuredScoreDetail.Categories"/>), son libellé, sa forme de saisie et,
/// pour <see cref="CategoryInputShape.Achieved"/>, la valeur fixe qu'elle accorde.
/// <see cref="SectionKey"/> regroupe optionnellement les catégories à l'affichage (ex: "Section haute"
/// au Yams) ; laissé à <c>null</c>, la catégorie s'affiche à plat (ex: Akropolis).
/// <see cref="AccentColor"/> est une couleur CSS optionnelle (valeur --accent par défaut si absente).
/// <see cref="Icon"/> est un émoji optionnel affiché à côté du libellé (ex: face de dé au Yams).
/// </summary>
public record CategoryDefinition(
    string Key,
    string Label,
    CategoryInputShape Shape,
    int? FixedPoints = null,
    string? SectionKey = null,
    string? AccentColor = null,
    string? Icon = null);
