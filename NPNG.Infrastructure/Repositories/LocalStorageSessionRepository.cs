using System.Collections.Immutable;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository de session utilisant le LocalStorage du navigateur via JSInterop.
/// Respecte l'architecture : l'Infrastructure dépend de l'Application et du Domaine.
/// </summary>
public class LocalStorageSessionRepository(ILocalStorageService localStorage) : ISessionRepository
{
    private const string StorageKeyPrefix = "npng_session_";

    public Task SaveSessionAsync(Session session) =>
        localStorage.SetItemAsync($"{StorageKeyPrefix}{session.Id}", session);

    public async Task<Session?> GetSessionAsync(Guid id)
    {
        var session = await localStorage.GetItemAsync<Session>($"{StorageKeyPrefix}{id}");
        return session is null ? null : NormalizeLegacySession(session);
    }

    public async Task<IEnumerable<Session>> GetAllSessionsAsync()
    {
        var sessions = await localStorage.GetAllItemsAsync<Session>(StorageKeyPrefix);
        return sessions.Select(NormalizeLegacySession).OrderByDescending(s => s.StartedAt);
    }

    public Task DeleteSessionAsync(Guid id) =>
        localStorage.RemoveItemAsync($"{StorageKeyPrefix}{id}");

    /// <summary>
    /// Répare les sessions sérialisées avant l'introduction de Session.Teams : un ImmutableArray&lt;T&gt;
    /// absent du JSON historique se désérialise vers sa valeur par défaut "cassée" (IsDefault == true),
    /// qui plante au premier accès (.Length, .Any(), foreach...) plutôt que de se comporter comme un
    /// tableau vide. C'est le seul endroit du code où cet état est toléré.
    /// </summary>
    private static Session NormalizeLegacySession(Session session) =>
        session.Teams.IsDefault ? session with { Teams = ImmutableArray<Team>.Empty } : session;
}
