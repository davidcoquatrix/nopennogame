namespace NPNG.Domain.Entities;

/// <summary>
/// Bonus conditionnel accordé quand le sous-total d'une section de catégories atteint
/// <see cref="Threshold"/> (ex: +35 points au Yams si la section haute atteint 63).
/// Purement informatif pour l'UI (affichage de la progression) ; le calcul réel du bonus
/// reste dans <see cref="Services.StructuredScoringDefinition.CalculateTotal"/>.
/// </summary>
public record SectionBonus(int Threshold, int Points);
