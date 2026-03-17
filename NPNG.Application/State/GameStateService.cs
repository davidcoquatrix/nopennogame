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
    /// Tente de recharger la dernière session active depuis le repository.
    /// Retourne true si une session a été restaurée, false sinon.
    /// </summary>
    public async Task<bool> LoadLatestActiveSessionAsync()
    {
        var sessions = await _sessionRepository.GetAllSessionsAsync();
        var latestActive = sessions.FirstOrDefault(s => s.Status == SessionStatus.Active || s.Status == SessionStatus.Setup);
        
        if (latestActive != null)
        {
            CurrentSession = latestActive;
            NotifyStateChanged();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Étape 1 : Initialise une nouvelle partie (en attente de joueurs).
    /// </summary>
    public async Task InitializeNewSessionAsync(GameTemplate template)
    {
        CurrentSession = Session.Create(template);
        await SaveStateAsync();
    }

    /// <summary>
    /// Étape 2 : Ajoute un joueur à la session en cours d'initialisation.
    /// </summary>
    public async Task AddPlayerToSetupAsync(string name, string emoji, string color)
    {
        if (CurrentSession is null || (CurrentSession.Status != SessionStatus.Active && CurrentSession.Status != SessionStatus.Setup)) return;

        var displayOrder = CurrentSession.Players.Length;
        // First player is the first one added by default
        var isFirst = displayOrder == 0; 

        var newPlayer = new SessionPlayer(Guid.NewGuid(), name, emoji, displayOrder, color, isFirst);
        
        CurrentSession = CurrentSession with 
        { 
            Players = CurrentSession.Players.Add(newPlayer) 
        };
        
        await SaveStateAsync();
    }

    /// <summary>
    /// Retire un joueur lors du setup.
    /// </summary>
    public async Task RemovePlayerFromSetupAsync(Guid playerId)
    {
        if (CurrentSession is null || (CurrentSession.Status != SessionStatus.Active && CurrentSession.Status != SessionStatus.Setup)) return;

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
            await SaveStateAsync();
        }
    }

    /// <summary>
    /// Force manuellement le statut de Premier Joueur.
    /// </summary>
    public async Task SetFirstPlayerManuallyAsync(Guid playerId)
    {
        if (CurrentSession is null || (CurrentSession.Status != SessionStatus.Active && CurrentSession.Status != SessionStatus.Setup)) return;

        var updatedPlayers = CurrentSession.Players.Select(p => 
            p with { IsFirstPlayer = p.PlayerId == playerId }
        ).ToImmutableArray();

        CurrentSession = CurrentSession with { Players = updatedPlayers };
        await SaveStateAsync();
    }

    /// <summary>
    /// Déplace un joueur vers le haut dans l'ordre d'affichage.
    /// </summary>
    public async Task MovePlayerUpAsync(Guid playerId)
    {
        if (CurrentSession is null || (CurrentSession.Status != SessionStatus.Active && CurrentSession.Status != SessionStatus.Setup)) return;

        var playersList = CurrentSession.Players.OrderBy(p => p.DisplayOrder).ToList();
        var index = playersList.FindIndex(p => p.PlayerId == playerId);

        if (index > 0)
        {
            var player = playersList[index];
            playersList.RemoveAt(index);
            playersList.Insert(index - 1, player);

            var updatedPlayers = playersList
                .Select((p, i) => p with { DisplayOrder = i })
                .ToImmutableArray();

            CurrentSession = CurrentSession with { Players = updatedPlayers };
            await SaveStateAsync();
        }
    }

    /// <summary>
    /// Déplace un joueur vers le bas dans l'ordre d'affichage.
    /// </summary>
    public async Task MovePlayerDownAsync(Guid playerId)
    {
        if (CurrentSession is null || (CurrentSession.Status != SessionStatus.Active && CurrentSession.Status != SessionStatus.Setup)) return;

        var playersList = CurrentSession.Players.OrderBy(p => p.DisplayOrder).ToList();
        var index = playersList.FindIndex(p => p.PlayerId == playerId);

        if (index >= 0 && index < playersList.Count - 1)
        {
            var player = playersList[index];
            playersList.RemoveAt(index);
            playersList.Insert(index + 1, player);

            var updatedPlayers = playersList
                .Select((p, i) => p with { DisplayOrder = i })
                .ToImmutableArray();

            CurrentSession = CurrentSession with { Players = updatedPlayers };
            await SaveStateAsync();
        }
    }

    public async Task StartSessionAsync()
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Setup) return;
        
        CurrentSession = CurrentSession with 
        { 
            Status = SessionStatus.Active,
            StartedAt = DateTime.UtcNow // Reset start time to when actually started
        };
        
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
    /// Gère également la condition de fin de partie si les règles sont atteintes.
    /// </summary>
    public async Task AdvanceToNextRoundAsync()
    {
        if (CurrentSession is null || CurrentSession.Status != SessionStatus.Active) return;

        bool isGameFinished = false;

        if (!CurrentSession.RulesOverridden)
        {
            // Check if MaxRounds is reached
            if (CurrentSession.Template.Rules?.MaxRounds.HasValue == true)
            {
                if (CurrentSession.CurrentRound >= CurrentSession.Template.Rules.MaxRounds.Value)
                {
                    isGameFinished = true;
                }
            }

            // Check if TargetScore is reached by any player
            if (!isGameFinished && CurrentSession.Template.Rules?.TargetScore.HasValue == true)
            {
                var target = CurrentSession.Template.Rules.TargetScore.Value;
                var playerIds = CurrentSession.Players.Select(p => p.PlayerId);
                var leaderboard = Domain.Services.ScoreCalculator.CalculateLeaderboard(
                    CurrentSession.Template.ScoreType,
                    playerIds,
                    CurrentSession.Scores);

                if (leaderboard.Any(l => l.TotalScore >= target))
                {
                    isGameFinished = true;
                }
            }
        }

        if (isGameFinished)
        {
            CurrentSession = CurrentSession with
            {
                Status = SessionStatus.Finished,
                EndedAt = DateTime.UtcNow
            };
        }
        else
        {
            // Find current first player
            var currentFirstIndex = CurrentSession.Players.ToList().FindIndex(p => p.IsFirstPlayer);
            
            var updatedPlayers = CurrentSession.Players.ToList();
            
            if (currentFirstIndex >= 0 && updatedPlayers.Count > 0)
            {
                // Remove first player flag from current
                updatedPlayers[currentFirstIndex] = updatedPlayers[currentFirstIndex] with { IsFirstPlayer = false };
                
                var mechanic = CurrentSession.Template.Rules?.FirstPlayerMechanic ?? FirstPlayerMechanic.Sequential;
                var nextIndex = currentFirstIndex;

                if (mechanic == FirstPlayerMechanic.Sequential)
                {
                    // Assign to next player (looping back to 0)
                    nextIndex = (currentFirstIndex + 1) % updatedPlayers.Count;
                }
                else if (mechanic == FirstPlayerMechanic.Winner || mechanic == FirstPlayerMechanic.Loser)
                {
                    var playerIds = CurrentSession.Players.Select(p => p.PlayerId);
                    var leaderboard = Domain.Services.ScoreCalculator.CalculateLeaderboard(
                        CurrentSession.Template.ScoreType,
                        playerIds,
                        CurrentSession.Scores);

                    if (leaderboard.Any())
                    {
                        var targetPlayerId = mechanic == FirstPlayerMechanic.Winner
                            ? leaderboard.First().PlayerId
                            : leaderboard.Last().PlayerId;
                            
                        var foundIndex = updatedPlayers.FindIndex(p => p.PlayerId == targetPlayerId);
                        if (foundIndex >= 0)
                        {
                            nextIndex = foundIndex;
                        }
                    }
                }
                else if (mechanic == FirstPlayerMechanic.HighestInPreviousRound || mechanic == FirstPlayerMechanic.LowestInPreviousRound)
                {
                    var previousRoundScores = CurrentSession.Scores
                        .Where(s => s.Round == CurrentSession.CurrentRound)
                        .ToList();

                    if (previousRoundScores.Any())
                    {
                        var orderedScores = previousRoundScores.OrderByDescending(s => s.Value).ToList();
                        var targetPlayerId = mechanic == FirstPlayerMechanic.HighestInPreviousRound
                            ? orderedScores.First().PlayerId
                            : orderedScores.Last().PlayerId;

                        var foundIndex = updatedPlayers.FindIndex(p => p.PlayerId == targetPlayerId);
                        if (foundIndex >= 0)
                        {
                            nextIndex = foundIndex;
                        }
                    }
                }
                // If mechanic == None, nextIndex remains currentFirstIndex (no change)

                updatedPlayers[nextIndex] = updatedPlayers[nextIndex] with { IsFirstPlayer = true };
            }

            CurrentSession = CurrentSession with 
            { 
                CurrentRound = CurrentSession.CurrentRound + 1,
                Players = updatedPlayers.ToImmutableArray()
            };
        }

        await SaveStateAsync();
    }

    /// <summary>
    /// Force la fin de la partie manuellement.
    /// </summary>
    public async Task FinishSessionAsync()
    {
        if (CurrentSession is null || CurrentSession.Status == SessionStatus.Finished) return;
        
        CurrentSession = CurrentSession with 
        { 
            Status = SessionStatus.Finished,
            EndedAt = DateTime.UtcNow
        };
        
        await SaveStateAsync();
    }

    /// <summary>
    /// Reprend une partie terminée en ignorant les règles de fin automatique.
    /// </summary>
    public async Task ResumeSessionAsync()
    {
        if (CurrentSession is null) return;
        
        CurrentSession = CurrentSession with 
        { 
            Status = SessionStatus.Active,
            EndedAt = null,
            RulesOverridden = true
        };
        
        // Comme on reprend, on passe le tour qui a déclenché la fin
        // On fait avancer le FirstPlayer manuellement pour préparer le prochain tour
        var currentFirstIndex = CurrentSession.Players.ToList().FindIndex(p => p.IsFirstPlayer);
        var updatedPlayers = CurrentSession.Players.ToList();
        
        if (currentFirstIndex >= 0 && updatedPlayers.Count > 0)
        {
            updatedPlayers[currentFirstIndex] = updatedPlayers[currentFirstIndex] with { IsFirstPlayer = false };
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
    /// Abandonne la partie en cours.
    /// </summary>
    public async Task AbandonSessionAsync()
    {
        if (CurrentSession is null || CurrentSession.Status == SessionStatus.Abandoned) return;
        
        CurrentSession = CurrentSession with 
        { 
            Status = SessionStatus.Abandoned,
            EndedAt = DateTime.UtcNow
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
