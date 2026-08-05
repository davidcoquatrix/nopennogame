using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Application.State;

/// <summary>
/// Expose l'historique des parties terminées ou abandonnées (lecture et suppression).
/// Séparé de GameStateService, qui gère uniquement la session en cours.
/// </summary>
public class SessionHistoryService(ISessionRepository sessionRepository, GameStateService gameState)
{
    /// <summary>
    /// Retourne les sessions pertinentes pour l'historique, triées de la plus récente à la plus ancienne.
    /// </summary>
    public async Task<IReadOnlyList<Session>> GetHistoryAsync()
    {
        var allSessions = await sessionRepository.GetAllSessionsAsync();

        return allSessions
            .Where(SessionHistoryFilter.IsRelevantForHistory)
            .OrderByDescending(s => s.StartedAt)
            .ToList();
    }

    /// <summary>
    /// Récupère une session par son id, quel que soit son statut.
    /// </summary>
    public Task<Session?> GetSessionByIdAsync(Guid sessionId) => sessionRepository.GetSessionAsync(sessionId);

    /// <summary>
    /// Supprime définitivement une session de l'historique.
    /// </summary>
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        await sessionRepository.DeleteSessionAsync(sessionId);
        gameState.ClearCurrentSessionIfMatches(sessionId);
    }
}
