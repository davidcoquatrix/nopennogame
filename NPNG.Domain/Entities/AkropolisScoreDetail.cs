namespace NPNG.Domain.Entities;

/// <summary>
/// Un couple (multiplicateur, nombre de quartiers) pour une catégorie de score Akropolis.
/// </summary>
public record AkropolisCategoryScore(int Multiplicateur, int Quartiers)
{
    public int Subtotal => Multiplicateur * Quartiers;
}

/// <summary>
/// Détail du score de fin de partie pour Akropolis (ScoreType.Structured).
/// Persisté (pas seulement le total) afin de rester éditable via Time Travel.
/// </summary>
public record AkropolisScoreDetail(
    AkropolisCategoryScore Carrieres,
    AkropolisCategoryScore Habitations,
    AkropolisCategoryScore Marches,
    AkropolisCategoryScore Casernes,
    AkropolisCategoryScore Jardins,
    int PierresRestantes)
{
    public int Total =>
        Carrieres.Subtotal + Habitations.Subtotal + Marches.Subtotal +
        Casernes.Subtotal + Jardins.Subtotal + PierresRestantes;

    public static AkropolisScoreDetail Empty { get; } = new(
        new(0, 0), new(0, 0), new(0, 0), new(0, 0), new(0, 0), 0);
}
