using System.Collections.Immutable;

namespace NPNG.Domain.Entities;

/// <summary>
/// Les 10 combinaisons officielles du jeu Phase 10, dans l'ordre où elles doivent être tentées.
/// Donnée statique spécifique à ce jeu, utilisée uniquement pour l'affichage (comme
/// <see cref="CategoryDefinition.Label"/> pour Akropolis/Yams) — la logique de progression elle-même
/// (<see cref="Services.PhaseProgressCalculator"/>) reste générique et ignore ce contenu.
/// </summary>
public static class Phase10Phases
{
    public const int TotalPhases = 10;

    public static readonly ImmutableArray<string> Descriptions =
    [
        "2 groupes de 3",
        "1 groupe de 3 + 1 suite de 4",
        "1 groupe de 4 + 1 suite de 4",
        "1 suite de 7",
        "1 suite de 8",
        "1 suite de 9",
        "2 groupes de 4",
        "7 cartes de la même couleur",
        "1 groupe de 5 + 1 groupe de 2",
        "1 groupe de 5 + 1 groupe de 3"
    ];
}
