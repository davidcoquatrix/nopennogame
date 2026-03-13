using NPNG.Domain.Entities;

namespace NPNG.Application.Interfaces;

/// <summary>
/// Contrat pour la persistance des sessions de jeu.
/// Sera implémenté dans l'Infrastructure (par exemple, via LocalStorage pour le MVP).
/// </summary>
public interface ISessionRepository
{
    Task SaveSessionAsync(Session session);
    Task<Session?> GetSessionAsync(Guid id);
    Task<IEnumerable<Session>> GetAllSessionsAsync();
}
