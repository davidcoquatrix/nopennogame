using NPNG.Domain.Enums;

namespace NPNG.Domain.Services;

/// <summary>
/// Registre des jeux à score structuré : associe chaque <see cref="StructuredGameKind"/> à sa
/// <see cref="StructuredScoringDefinition"/>. Ajouter un nouveau jeu structuré revient à créer sa
/// définition et l'enregistrer ici.
/// </summary>
public static class StructuredScoringCatalogue
{
    private static readonly IReadOnlyDictionary<StructuredGameKind, StructuredScoringDefinition> Definitions =
        new Dictionary<StructuredGameKind, StructuredScoringDefinition>
        {
            [StructuredGameKind.Akropolis] = new AkropolisScoringDefinition(),
            [StructuredGameKind.Yams] = new YamsScoringDefinition()
        };

    public static StructuredScoringDefinition Get(StructuredGameKind kind) => Definitions[kind];
}
