using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Services;

/// <summary>
/// Décrit un jeu à score structuré (ex: Akropolis, Yams) : ses catégories et la règle de calcul du
/// total à partir d'un <see cref="StructuredScoreDetail"/>. Le calcul par défaut (somme de chaque
/// catégorie selon sa <see cref="CategoryInputShape"/>) couvre les jeux sans règle additionnelle
/// (ex: Akropolis) ; les jeux avec un bonus conditionnel (ex: Yams) surchargent <see cref="CalculateTotal"/>.
/// </summary>
public abstract class StructuredScoringDefinition
{
    public abstract StructuredGameKind Kind { get; }

    public abstract IReadOnlyList<CategoryDefinition> Categories { get; }

    /// <summary>
    /// Style d'affichage de la feuille de score pour ce jeu. Par défaut <see cref="StructuredLayoutStyle.Cards"/>
    /// (ex: Akropolis) ; les jeux à nombreuses catégories à valeur unique (ex: Yams) surchargent en <see cref="StructuredLayoutStyle.Table"/>.
    /// </summary>
    public virtual StructuredLayoutStyle LayoutStyle => StructuredLayoutStyle.Cards;

    public virtual int CalculateTotal(StructuredScoreDetail detail) =>
        Categories.Sum(category => CategoryScore(category, detail.GetValue(category.Key)));

    /// <summary>
    /// Sous-total des catégories appartenant à <paramref name="sectionKey"/> (ou sans section si <c>null</c>),
    /// utilisé par l'UI pour afficher un sous-total par section (ex: progression du bonus au Yams).
    /// </summary>
    public int SectionTotal(string? sectionKey, StructuredScoreDetail detail) =>
        Categories
            .Where(category => category.SectionKey == sectionKey)
            .Sum(category => CategoryScore(category, detail.GetValue(category.Key)));

    /// <summary>
    /// Bonus conditionnel à afficher pour une section donnée (ex: le +35 au Yams si la section haute
    /// atteint 63), <c>null</c> si la section n'a pas de bonus. Purement informatif pour l'UI.
    /// </summary>
    public virtual SectionBonus? GetSectionBonus(string? sectionKey) => null;

    protected static int CategoryScore(CategoryDefinition category, CategoryValue value) => category.Shape switch
    {
        CategoryInputShape.FreeValue => value.A,
        CategoryInputShape.MultiplierByCount => value.A * value.B,
        CategoryInputShape.Achieved => value.A != 0 ? category.FixedPoints ?? 0 : 0,
        _ => throw new NotImplementedException($"La forme de saisie {category.Shape} n'est pas gérée.")
    };
}
