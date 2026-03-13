using System.Collections.Immutable;
using NPNG.Application.Interfaces;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Application.State;

/// <summary>
/// State Manager de l'application. Gère la session active en mémoire et orchestre les cas d'usage.
/// L'UI (Blazor) s'y abonne via l'événement OnStateChanged.
/// </summary>
public class GameStateService
{
    private readonly ISessionRepository _sessionRepository;

    public GameStateService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// La session actuellement jouée (peut être nulle si aucune partie n'est en cours).
    /// </summary>
    public Session? CurrentSession { get; private set; }

    /// <summary>
    /// Historique des scores de la session actuelle.
    /// </summary>
    public ImmutableList<ScoreEntry> CurrentScores { get; private set; } = ImmutableList<ScoreEntry>.Empty;

    /// <summary>
    /// Événement déclenché à chaque modification de l'état pour que l'UI se mette à jour.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Initialise une nouvelle partie.
    /// </summary>
    public async Task StartNewSessionAsync(GameTemplate template, IEnumerable<Player> players)
    {
        var sessionPlayers = players.Select((p, index) => 
            new SessionPlayer(p.Id, index + 1, "#FFFFFF") // TODO: Assigner des couleurs uniques dynamiquement
        ).ToImmutableArray();

        CurrentSession = new Session(
            Id: Guid.NewGuid(),
            TemplateId: template.Id,
            StartedAt: DateTime.UtcNow,
            Players: sessionPlayers
        );

        CurrentScores = ImmutableList<ScoreEntry>.Empty;

        await SaveStateAsync();
    }

    /// <summary>
    /// Ajoute ou met à jour un score pour un joueur lors d'une manche (Gère la saisie rapide et le Time Travel).
    /// </summary>
    public async Task RecordScoreAsync(Guid playerId, int round, int value)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Aucune session active en cours.");
        }

        // Vérifier si un score existe déjà pour ce joueur à ce tour (Time Travel / Édition)
        var existingScore = CurrentScores.FirstOrDefault(s => s.PlayerId == playerId && s.Round == round);

        if (existingScore is null)
        {
            // Nouveau score
            var newEntry = new ScoreEntry(Guid.NewGuid(), CurrentSession.Id, playerId, round, value);
            CurrentScores = CurrentScores.Add(newEntry);
        }
        else
        {
            // Modification (Time Travel)
            var updatedEntry = existingScore with { Value = value };
            CurrentScores = CurrentScores.Replace(existingScore, updatedEntry);
        }

        await SaveStateAsync();
    }

    /// <summary>
    /// Sauvegarde la session en cours via le repository et notifie l'UI du changement.
    /// </summary>
    private async Task SaveStateAsync()
    {
        if (CurrentSession is not null)
        {
            await _sessionRepository.SaveSessionAsync(CurrentSession);
        }
        
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
