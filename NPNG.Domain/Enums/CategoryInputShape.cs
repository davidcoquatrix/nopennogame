namespace NPNG.Domain.Enums;

/// <summary>
/// Décrit comment une catégorie de score structuré (<see cref="Entities.CategoryDefinition"/>) se saisit
/// et se calcule à partir de sa <see cref="Entities.CategoryValue"/>.
/// </summary>
public enum CategoryInputShape
{
    /// <summary>
    /// Un seul nombre saisi librement (ex: Pierres restantes, Chance au Yams).
    /// </summary>
    FreeValue,

    /// <summary>
    /// Deux nombres (multiplicateur × quantité), le sous-total est leur produit (ex: catégories Akropolis).
    /// </summary>
    MultiplierByCount,

    /// <summary>
    /// Un simple "obtenu / pas obtenu" qui accorde une valeur fixe (<see cref="Entities.CategoryDefinition.FixedPoints"/>)
    /// si atteint, 0 sinon (ex: Full/Suites/Yams au Yams).
    /// </summary>
    Achieved
}
