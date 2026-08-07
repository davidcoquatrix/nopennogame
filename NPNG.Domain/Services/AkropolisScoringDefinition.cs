using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Services;

/// <summary>
/// Catégories de score d'Akropolis : 5 catégories multiplicateur × quartiers, plus les pierres
/// restantes en valeur libre. Le total (somme par défaut de <see cref="StructuredScoringDefinition"/>)
/// n'a pas besoin de règle additionnelle.
/// </summary>
public sealed class AkropolisScoringDefinition : StructuredScoringDefinition
{
    public static class Keys
    {
        public const string Carrieres = "Carrieres";
        public const string Habitations = "Habitations";
        public const string Marches = "Marches";
        public const string Casernes = "Casernes";
        public const string Jardins = "Jardins";
        public const string PierresRestantes = "PierresRestantes";
    }

    public override StructuredGameKind Kind => StructuredGameKind.Akropolis;

    public override StructuredLayoutStyle LayoutStyle => StructuredLayoutStyle.Table;

    public override IReadOnlyList<CategoryDefinition> Categories { get; } =
    [
        new(Keys.Carrieres, "Carrières", CategoryInputShape.MultiplierByCount, AccentColor: "#60A5FA"),
        new(Keys.Habitations, "Habitations", CategoryInputShape.MultiplierByCount, AccentColor: "#FCD34D"),
        new(Keys.Marches, "Marchés", CategoryInputShape.MultiplierByCount, AccentColor: "var(--warn)"),
        new(Keys.Casernes, "Casernes", CategoryInputShape.MultiplierByCount, AccentColor: "#A78BFA"),
        new(Keys.Jardins, "Jardins", CategoryInputShape.MultiplierByCount, AccentColor: "var(--success)"),
        new(Keys.PierresRestantes, "Pierres restantes", CategoryInputShape.FreeValue, AccentColor: "var(--sub)")
    ];
}
