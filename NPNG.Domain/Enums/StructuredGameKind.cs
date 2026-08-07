namespace NPNG.Domain.Enums;

/// <summary>
/// Identifie le jeu à score structuré (<see cref="ScoreType.Structured"/>) auquel appartient un
/// <see cref="Entities.GameTemplate"/>, et donc quelle <see cref="Services.StructuredScoringDefinition"/>
/// (catégories + règle de calcul) lui appliquer.
/// </summary>
public enum StructuredGameKind
{
    Akropolis,
    Yams
}
