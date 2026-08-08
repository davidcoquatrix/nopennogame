using NPNG.Domain.Entities;

namespace NPNG.Domain.Services;

/// <summary>
/// Service pur du domaine responsable de dériver la progression de phase d'un joueur à partir de son
/// historique de <see cref="ScoreEntry"/> (source de vérité, pour rester cohérent avec Time Travel).
/// Générique : ne dépend d'aucune donnée propre à un jeu précis (ex: <see cref="Phase10Phases"/>).
/// </summary>
public static class PhaseProgressCalculator
{
    /// <summary>
    /// Nombre de phases validées par le joueur sur l'ensemble de la partie.
    /// </summary>
    public static int GetCompletedPhaseCount(Guid playerId, IEnumerable<ScoreEntry> entries) =>
        entries.Count(e => e.PlayerId == playerId && e.PhaseDetail?.Completed == true);

    /// <summary>
    /// Phase que le joueur est en train de tenter (la suivante après ses phases déjà validées),
    /// plafonnée à <paramref name="totalPhases"/>.
    /// </summary>
    public static int GetCurrentPhase(Guid playerId, IEnumerable<ScoreEntry> entries, int totalPhases) =>
        Math.Min(GetCompletedPhaseCount(playerId, entries) + 1, totalPhases);

    /// <summary>
    /// Indique si le joueur a validé la phase gagnante, mettant fin à la partie.
    /// </summary>
    public static bool HasWon(Guid playerId, IEnumerable<ScoreEntry> entries, int winningPhase) =>
        GetCompletedPhaseCount(playerId, entries) >= winningPhase;
}
