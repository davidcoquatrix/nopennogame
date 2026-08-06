using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.JSInterop;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;

namespace NPNG.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository de session utilisant le LocalStorage du navigateur via JSInterop.
/// Respecte l'architecture : l'Infrastructure dépend de l'Application et du Domaine.
/// </summary>
public class LocalStorageSessionRepository(IJSRuntime jsRuntime) : ISessionRepository
{
    private const string StorageKeyPrefix = "npng_session_";
    
    // Options de sérialisation pour gérer les constructeurs des records et l'immuabilité
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SaveSessionAsync(Session session)
    {
        var key = $"{StorageKeyPrefix}{session.Id}";
        var json = JsonSerializer.Serialize(session, _jsonOptions);
        
        // Appel natif à l'API javascript window.localStorage
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task<Session?> GetSessionAsync(Guid id)
    {
        var key = $"{StorageKeyPrefix}{id}";
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            var session = JsonSerializer.Deserialize<Session>(json, _jsonOptions);
            return session is null ? null : NormalizeLegacySession(session);
        }
        catch (JsonException)
        {
            // En cas de corruption des données dans le localStorage
            return null;
        }
    }

    public async Task<IEnumerable<Session>> GetAllSessionsAsync()
    {
        // Pour récupérer toutes les sessions, on utilise un petit bout de JS inline 
        // pour filtrer les clés du localStorage qui commencent par notre préfixe.
        var script = $$"""
            (() => {
                const sessions = [];
                for (let i = 0; i < localStorage.length; i++) {
                    const key = localStorage.key(i);
                    if (key.startsWith('{{StorageKeyPrefix}}')) {
                        sessions.push(localStorage.getItem(key));
                    }
                }
                return sessions;
            })()
            """;

        var jsonArray = await jsRuntime.InvokeAsync<string[]>("eval", script);
        
        var sessions = new List<Session>();
        foreach (var json in jsonArray)
        {
            try
            {
                var session = JsonSerializer.Deserialize<Session>(json, _jsonOptions);
                if (session is not null)
                {
                    sessions.Add(NormalizeLegacySession(session));
                }
            }
            catch (JsonException)
            {
                // Ignorer les entrées corrompues
            }
        }

        return sessions.OrderByDescending(s => s.StartedAt);
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        var key = $"{StorageKeyPrefix}{id}";
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    /// <summary>
    /// Répare les sessions sérialisées avant l'introduction de Session.Teams : un ImmutableArray&lt;T&gt;
    /// absent du JSON historique se désérialise vers sa valeur par défaut "cassée" (IsDefault == true),
    /// qui plante au premier accès (.Length, .Any(), foreach...) plutôt que de se comporter comme un
    /// tableau vide. C'est le seul endroit du code où cet état est toléré.
    /// </summary>
    private static Session NormalizeLegacySession(Session session) =>
        session.Teams.IsDefault ? session with { Teams = ImmutableArray<Team>.Empty } : session;
}
