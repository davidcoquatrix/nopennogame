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
    /// Événement déclenché à chaque modification de l'état pour que l'UI se mette à jour.
    /// </summary>
    public event Action? OnStateChanged;

    /// <summary>
    /// Étape 1 : Initialise une nouvelle partie (en attente de joueurs).
    /// </summary>
    public void InitializeNewSession(GameTemplate template)
    {
        CurrentSession = Session.Create(template);
        NotifyStateChanged();
    }

    /// <summary>
    /// Étape 2 : Ajoute un joueur à la session en cours d'initialisation.
    /// </summary>
    public void AddPlayerToSetup(string name, string emoji, string color)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        var displayOrder = CurrentSession.Players.Length;
        // First player is the first one added by default
        var isFirst = displayOrder == 0; 

        var newPlayer = new SessionPlayer(Guid.NewGuid(), name, emoji, displayOrder, color, isFirst);
        
        CurrentSession = CurrentSession with 
        { 
            Players = CurrentSession.Players.Add(newPlayer) 
        };
        
        NotifyStateChanged();
    }

    /// <summary>
    /// Retire un joueur lors du setup.
    /// </summary>
    public void RemovePlayerFromSetup(Guid playerId)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        var playerToRemove = CurrentSession.Players.FirstOrDefault(p => p.PlayerId == playerId);
        if (playerToRemove is not null)
        {
            var updatedPlayers = CurrentSession.Players.Remove(playerToRemove);
            
            // Re-assign FirstPlayer if the removed one was it, and someone is left
            if (playerToRemove.IsFirstPlayer && updatedPlayers.Length > 0)
            {
                var newFirst = updatedPlayers[0] with { IsFirstPlayer = true };
                updatedPlayers = updatedPlayers.Replace(updatedPlayers[0], newFirst);
            }
            
            // Re-order remaining players to avoid gaps
            var reorderedPlayers = updatedPlayers
                .OrderBy(p => p.DisplayOrder)
                .Select((p, index) => p with { DisplayOrder = index })
                .ToImmutableArray();

            CurrentSession = CurrentSession with { Players = reorderedPlayers };
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Force manuellement le statut de Premier Joueur.
    /// </summary>
    public void SetFirstPlayerManually(Guid playerId)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        var updatedPlayers = CurrentSession.Players.Select(p => 
            p with { IsFirstPlayer = p.PlayerId == playerId }
        ).ToImmutableArray();

        CurrentSession = CurrentSession with { Players = updatedPlayers };
        NotifyStateChanged();
    }

    /// <summary>
    /// Réordonne les joueurs par index exact (plus robuste pour le Drag & Drop).
    /// </summary>
    public void ReorderPlayerByIndex(Guid draggedPlayerId, int dropIndex)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        var playersList = CurrentSession.Players.OrderBy(p => p.DisplayOrder).ToList();
        var draggedPlayer = playersList.FirstOrDefault(p => p.PlayerId == draggedPlayerId);

        if (draggedPlayer is not null)
        {
            int originalSourceIndex = playersList.IndexOf(draggedPlayer);
            playersList.Remove(draggedPlayer);

            // On décale l'index de drop si l'élément qu'on retire était avant
            if (dropIndex > originalSourceIndex)
            {
                dropIndex--;
            }

            if (dropIndex < 0) dropIndex = 0;
            if (dropIndex > playersList.Count) dropIndex = playersList.Count;

            playersList.Insert(dropIndex, draggedPlayer);

            var updatedPlayers = playersList
                .Select((p, index) => p with { DisplayOrder = index })
                .ToImmutableArray();

            CurrentSession = CurrentSession with { Players = updatedPlayers };
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Réordonne les joueurs suite à un Drag & Drop.
    /// Si targetPlayerId est null, on place le joueur à la fin.
    /// </summary>
    public void ReorderPlayer(Guid draggedPlayerId, Guid? targetPlayerId)
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        var playersList = CurrentSession.Players.OrderBy(p => p.DisplayOrder).ToList();
        var draggedPlayer = playersList.FirstOrDefault(p => p.PlayerId == draggedPlayerId);

        if (draggedPlayer is not null)
        {
            // On mémorise si on déplace vers le bas pour ajuster l'index d'insertion
            int originalSourceIndex = playersList.IndexOf(draggedPlayer);
            int originalTargetIndex = targetPlayerId.HasValue 
                ? playersList.FindIndex(p => p.PlayerId == targetPlayerId.Value) 
                : -1;
            
            bool isMovingDown = originalSourceIndex < originalTargetIndex;

            playersList.Remove(draggedPlayer);
            
            if (targetPlayerId.HasValue)
            {
                var newTargetIndex = playersList.FindIndex(p => p.PlayerId == targetPlayerId.Value);
                if (newTargetIndex >= 0)
                {
                    // Si on descend dans la liste, on insère après la cible (car l'UI montre l'indicateur en dessous)
                    if (isMovingDown)
                    {
                        playersList.Insert(newTargetIndex + 1, draggedPlayer);
                    }
                    else
                    {
                        playersList.Insert(newTargetIndex, draggedPlayer);
                    }
                }
                else
                {
                    playersList.Add(draggedPlayer);
                }
            }
            else
            {
                // Drop at end
                playersList.Add(draggedPlayer);
            }

            var updatedPlayers = playersList
                .Select((p, index) => p with { DisplayOrder = index })
                .ToImmutableArray();

            CurrentSession = CurrentSession with { Players = updatedPlayers };
            NotifyStateChanged();
        }
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

        var existingScore = CurrentSession.Scores.FirstOrDefault(s => s.PlayerId == playerId && s.Round == round);
        ImmutableArray<ScoreEntry> newScores;

        if (existingScore is null)
        {
            // Nouveau score
            var newEntry = new ScoreEntry(Guid.NewGuid(), CurrentSession.Id, playerId, round, value);
            newScores = CurrentSession.Scores.Add(newEntry);
        }
        else
        {
            // Modification (Time Travel)
            var updatedEntry = existingScore with { Value = value };
            newScores = CurrentSession.Scores.Replace(existingScore, updatedEntry);
        }

        CurrentSession = CurrentSession with { Scores = newScores };
        await SaveStateAsync();
    }
    
    /// <summary>
    /// Valide le tour en cours et passe au suivant.
    /// Met à jour le FirstPlayer en fonction de l'ordre de la table.
    /// </summary>
    public async Task AdvanceToNextRoundAsync()
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        // Find current first player
        var currentFirstIndex = CurrentSession.Players.ToList().FindIndex(p => p.IsFirstPlayer);
        
        var updatedPlayers = CurrentSession.Players.ToList();
        
        if (currentFirstIndex >= 0 && updatedPlayers.Count > 0)
        {
            // Remove first player flag from current
            updatedPlayers[currentFirstIndex] = updatedPlayers[currentFirstIndex] with { IsFirstPlayer = false };
            
            // Assign to next player (looping back to 0)
            var nextIndex = (currentFirstIndex + 1) % updatedPlayers.Count;
            updatedPlayers[nextIndex] = updatedPlayers[nextIndex] with { IsFirstPlayer = true };
        }

        CurrentSession = CurrentSession with 
        { 
            CurrentRound = CurrentSession.CurrentRound + 1,
            Players = updatedPlayers.ToImmutableArray()
        };

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
