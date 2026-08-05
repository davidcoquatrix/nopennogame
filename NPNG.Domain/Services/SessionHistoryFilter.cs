using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Services;

/// <summary>
/// Détermine quelles sessions méritent d'apparaître dans l'historique des parties.
/// </summary>
public static class SessionHistoryFilter
{
    /// <summary>
    /// Une session est pertinente pour l'historique si elle est terminée normalement,
    /// ou si elle a été abandonnée après qu'au moins un score ait été enregistré
    /// (une session abandonnée sans aucun score n'est qu'un brouillon jamais joué).
    /// </summary>
    public static bool IsRelevantForHistory(Session session) =>
        session.Status == SessionStatus.Finished
        || (session.Status == SessionStatus.Abandoned && session.Scores.Length > 0);
}
