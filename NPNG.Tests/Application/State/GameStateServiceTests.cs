using System.Collections.Immutable;
using Moq;
using NPNG.Application.Interfaces;
using NPNG.Application.State;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;
using NPNG.Domain.Services;

namespace NPNG.Tests.Application.State;

public class GameStateServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly GameStateService _sut; // System Under Test

    public GameStateServiceTests()
    {
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _sut = new GameStateService(_sessionRepositoryMock.Object);
    }

    private GameTemplate CreateDummyTemplate()
    {
        return new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative);
    }

    [Fact]
    public async Task InitializeNewSessionAsync_ShouldCreateSessionInSetupStatus()
    {
        // Arrange
        var template = CreateDummyTemplate();

        // Act
        await _sut.InitializeNewSessionAsync(template);

        // Assert
        Assert.NotNull(_sut.CurrentSession);
        Assert.Equal(SessionStatus.Setup, _sut.CurrentSession.Status);
        Assert.Equal(template.Id, _sut.CurrentSession.Template.Id);
        _sessionRepositoryMock.Verify(repo => repo.SaveSessionAsync(It.IsAny<Session>()), Times.Once);
    }

    [Fact]
    public async Task InitializeNewSessionAsync_ShouldAbandonPreviousActiveSessionBeforeCreatingNew()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var previousSessionId = _sut.CurrentSession!.Id;

        // Act
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());

        // Assert
        Assert.NotEqual(previousSessionId, _sut.CurrentSession!.Id);
        _sessionRepositoryMock.Verify(repo => repo.SaveSessionAsync(It.Is<Session>(
            s => s.Id == previousSessionId && s.Status == SessionStatus.Abandoned)), Times.Once);
    }

    [Fact]
    public async Task AddPlayerToSetupAsync_ShouldAddPlayerAndAssignFirstPlayerToFirstAdded()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());

        // Act
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");

        // Assert
        var players = _sut.CurrentSession!.Players;
        Assert.Equal(2, players.Length);
        
        var alice = players.Single(p => p.Name == "Alice");
        var bob = players.Single(p => p.Name == "Bob");
        
        Assert.True(alice.IsFirstPlayer);
        Assert.False(bob.IsFirstPlayer);
        Assert.Equal(0, alice.DisplayOrder);
        Assert.Equal(1, bob.DisplayOrder);
    }

    [Fact]
    public async Task RemovePlayerFromSetupAsync_ShouldReassignFirstPlayerIfRemoved()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        
        var aliceId = _sut.CurrentSession!.Players.First(p => p.Name == "Alice").PlayerId;

        // Act
        await _sut.RemovePlayerFromSetupAsync(aliceId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Single(players);
        Assert.Equal("Bob", players[0].Name);
        Assert.True(players[0].IsFirstPlayer); // Bob should become the first player
        Assert.Equal(0, players[0].DisplayOrder); // Bob's order should be updated to 0
    }

    [Fact]
    public async Task SetFirstPlayerManuallyAsync_ShouldSetOnlyGivenPlayerAsFirst()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // Premier par défaut
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF");
        var charlieId = _sut.CurrentSession!.Players.Single(p => p.Name == "Charlie").PlayerId;

        // Act
        await _sut.SetFirstPlayerManuallyAsync(charlieId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.True(players.Single(p => p.Name == "Charlie").IsFirstPlayer);
        Assert.False(players.Single(p => p.Name == "Alice").IsFirstPlayer);
        Assert.False(players.Single(p => p.Name == "Bob").IsFirstPlayer);
    }

    [Fact]
    public async Task MovePlayerUpAsync_ShouldSwapWithPreviousPlayer()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // 0
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");   // 1
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // 2
        var bobId = _sut.CurrentSession!.Players.Single(p => p.Name == "Bob").PlayerId;

        // Act
        await _sut.MovePlayerUpAsync(bobId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Equal(0, players.Single(p => p.Name == "Bob").DisplayOrder);
        Assert.Equal(1, players.Single(p => p.Name == "Alice").DisplayOrder);
        Assert.Equal(2, players.Single(p => p.Name == "Charlie").DisplayOrder);
    }

    [Fact]
    public async Task MovePlayerUpAsync_WhenAlreadyFirst_ShouldNotChangeOrder()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // 0
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");   // 1
        var aliceId = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice").PlayerId;

        // Act
        await _sut.MovePlayerUpAsync(aliceId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Equal(0, players.Single(p => p.Name == "Alice").DisplayOrder);
        Assert.Equal(1, players.Single(p => p.Name == "Bob").DisplayOrder);
    }

    [Fact]
    public async Task MovePlayerDownAsync_ShouldSwapWithNextPlayer()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // 0
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");   // 1
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // 2
        var bobId = _sut.CurrentSession!.Players.Single(p => p.Name == "Bob").PlayerId;

        // Act
        await _sut.MovePlayerDownAsync(bobId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Equal(0, players.Single(p => p.Name == "Alice").DisplayOrder);
        Assert.Equal(1, players.Single(p => p.Name == "Charlie").DisplayOrder);
        Assert.Equal(2, players.Single(p => p.Name == "Bob").DisplayOrder);
    }

    [Fact]
    public async Task MovePlayerDownAsync_WhenAlreadyLast_ShouldNotChangeOrder()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // 0
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");   // 1
        var bobId = _sut.CurrentSession!.Players.Single(p => p.Name == "Bob").PlayerId;

        // Act
        await _sut.MovePlayerDownAsync(bobId);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Equal(0, players.Single(p => p.Name == "Alice").DisplayOrder);
        Assert.Equal(1, players.Single(p => p.Name == "Bob").DisplayOrder);
    }

    [Fact]
    public async Task StartSessionAsync_ShouldChangeStatusToActive()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");

        // Act
        await _sut.StartSessionAsync();

        // Assert
        Assert.Equal(SessionStatus.Active, _sut.CurrentSession!.Status);
    }

    [Fact]
    public async Task RecordScoreAsync_ShouldAddOrUpdateScoreEntry()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var aliceId = _sut.CurrentSession!.Players[0].PlayerId;

        // Act
        await _sut.RecordScoreAsync(aliceId, 1, 15);
        await _sut.RecordScoreAsync(aliceId, 1, 20); // Update round 1
        await _sut.RecordScoreAsync(aliceId, 2, 10); // Add round 2

        // Assert
        var scores = _sut.CurrentSession.Scores;
        Assert.Equal(2, scores.Length);
        Assert.Equal(20, scores.Single(s => s.Round == 1).Value);
        Assert.Equal(10, scores.Single(s => s.Round == 2).Value);
    }

    [Fact]
    public async Task AdvanceToNextRoundAsync_ShouldShiftFirstPlayerAndIncrementRound()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000"); // Index 0 (First)
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");   // Index 1
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // Index 2
        await _sut.StartSessionAsync();

        // Act - End of Round 1
        await _sut.AdvanceToNextRoundAsync();

        // Assert - Round 2
        Assert.Equal(2, _sut.CurrentSession!.CurrentRound);
        var playersAfterR1 = _sut.CurrentSession.Players.ToList();
        Assert.False(playersAfterR1.Single(p => p.Name == "Alice").IsFirstPlayer);
        Assert.True(playersAfterR1.Single(p => p.Name == "Bob").IsFirstPlayer); // Bob is next

        // Act - End of Round 2
        await _sut.AdvanceToNextRoundAsync();
        
        // Act - End of Round 3
        await _sut.AdvanceToNextRoundAsync();

        // Assert - Round 4 (Loop back to Alice)
        var playersAfterR3 = _sut.CurrentSession.Players.ToList();
        Assert.True(playersAfterR3.Single(p => p.Name == "Alice").IsFirstPlayer);
    }

    [Fact]
    public async Task AdvanceToNextRoundAsync_ShouldFinishSession_IfMaxRoundsReached()
    {
        // Arrange
        var rules = new GameRules(MaxRounds: 3);
        var template = new GameTemplate(Guid.NewGuid(), "Short Game", ScoreType.Cumulative, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();

        // Act - Play 3 rounds
        await _sut.AdvanceToNextRoundAsync(); // R1 -> R2
        await _sut.AdvanceToNextRoundAsync(); // R2 -> R3
        await _sut.AdvanceToNextRoundAsync(); // R3 -> Finish

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
        Assert.NotNull(_sut.CurrentSession.EndedAt);
    }

    [Theory]
    [InlineData(FirstPlayerMechanic.Winner)]
    [InlineData(FirstPlayerMechanic.HighestInPreviousRound)]
    public async Task AdvanceToNextRoundAsync_WithHighestScoreMechanic_MakesTopScorerTheNextFirstPlayer(FirstPlayerMechanic mechanic)
    {
        // Arrange
        var rules = new GameRules(FirstPlayerMechanic: mechanic);
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");   // Premier par défaut
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");     // Voisin séquentiel d'Alice
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // Meilleur score
        await _sut.StartSessionAsync();

        var players = _sut.CurrentSession!.Players;
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Alice").PlayerId, 1, 30);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Bob").PlayerId, 1, 10);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Charlie").PlayerId, 1, 50);

        // Act
        await _sut.AdvanceToNextRoundAsync();

        // Assert - pas Bob (le voisin séquentiel), mais Charlie (le vrai meilleur score)
        var updated = _sut.CurrentSession!.Players;
        Assert.True(updated.Single(p => p.Name == "Charlie").IsFirstPlayer);
        Assert.False(updated.Single(p => p.Name == "Bob").IsFirstPlayer);
    }

    [Theory]
    [InlineData(FirstPlayerMechanic.Loser)]
    [InlineData(FirstPlayerMechanic.LowestInPreviousRound)]
    public async Task AdvanceToNextRoundAsync_WithLowestScoreMechanic_MakesBottomScorerTheNextFirstPlayer(FirstPlayerMechanic mechanic)
    {
        // Arrange
        var rules = new GameRules(FirstPlayerMechanic: mechanic);
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");   // Premier par défaut
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");     // Voisin séquentiel d'Alice
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // Moins bon score
        await _sut.StartSessionAsync();

        var players = _sut.CurrentSession!.Players;
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Alice").PlayerId, 1, 30);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Bob").PlayerId, 1, 50);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Charlie").PlayerId, 1, 10);

        // Act
        await _sut.AdvanceToNextRoundAsync();

        // Assert - pas Bob (le voisin séquentiel), mais Charlie (le vrai moins bon score)
        var updated = _sut.CurrentSession!.Players;
        Assert.True(updated.Single(p => p.Name == "Charlie").IsFirstPlayer);
        Assert.False(updated.Single(p => p.Name == "Bob").IsFirstPlayer);
    }

    private static StructuredScoreDetail CreateAkropolisDetail(int carrieresMult, int carrieresCount, int pierresRestantes) =>
        new(ImmutableDictionary<string, CategoryValue>.Empty
            .Add(AkropolisScoringDefinition.Keys.Carrieres, new CategoryValue(carrieresMult, carrieresCount))
            .Add(AkropolisScoringDefinition.Keys.PierresRestantes, new CategoryValue(pierresRestantes)));

    [Fact]
    public async Task SubmitStructuredScoreAndFinishAsync_ShouldRecordDetailAtFixedRoundAndFinishSession()
    {
        // Arrange
        var template = new GameTemplate(Guid.NewGuid(), "Akropolis", ScoreType.Structured, StructuredKind: StructuredGameKind.Akropolis);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var aliceId = _sut.CurrentSession!.Players[0].PlayerId;

        var detail = CreateAkropolisDetail(2, 3, 4); // Total 10

        // Act
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, StructuredScoreDetail> { [aliceId] = detail });

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
        var entry = Assert.Single(_sut.CurrentSession.Scores);
        Assert.Equal(1, entry.Round);
        Assert.Equal(10, entry.Value);
        Assert.Equal(detail, entry.StructuredDetail);
    }

    [Fact]
    public async Task ResumeSessionAsync_WhenStructured_KeepsCurrentRoundFixed()
    {
        // Arrange
        var template = new GameTemplate(Guid.NewGuid(), "Akropolis", ScoreType.Structured, StructuredKind: StructuredGameKind.Akropolis);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var aliceId = _sut.CurrentSession!.Players[0].PlayerId;

        var detail = CreateAkropolisDetail(2, 3, 4); // Total 10
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, StructuredScoreDetail> { [aliceId] = detail });

        // Act - re-open for editing, then re-submit a correction
        await _sut.ResumeSessionAsync();
        var correctedDetail = CreateAkropolisDetail(2, 3, 9); // Total 15
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, StructuredScoreDetail> { [aliceId] = correctedDetail });

        // Assert - still a single round-1 entry, updated in place (no double-counting)
        Assert.Equal(1, _sut.CurrentSession!.CurrentRound);
        var entry = Assert.Single(_sut.CurrentSession.Scores);
        Assert.Equal(1, entry.Round);
        Assert.Equal(15, entry.Value);
    }

    [Fact]
    public async Task ResumeSessionAsync_WithWinnerMechanic_RespectsConfiguredMechanicNotSequentialOrder()
    {
        // Arrange
        var rules = new GameRules(MaxRounds: 1, FirstPlayerMechanic: FirstPlayerMechanic.Winner);
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");   // Premier par défaut
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");     // Voisin séquentiel d'Alice
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF"); // Vrai vainqueur
        await _sut.StartSessionAsync();

        var players = _sut.CurrentSession!.Players;
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Alice").PlayerId, 1, 30);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Bob").PlayerId, 1, 10);
        await _sut.RecordScoreAsync(players.Single(p => p.Name == "Charlie").PlayerId, 1, 50);

        // MaxRounds atteint : la partie se termine sans jamais avancer le premier joueur
        // (branche IsGameFinished de AdvanceToNextRoundAsync) — c'est ResumeSessionAsync
        // qui doit faire ce calcul en reprenant la partie.
        await _sut.AdvanceToNextRoundAsync();

        // Act
        await _sut.ResumeSessionAsync();

        // Assert - Charlie (le vainqueur) devient premier joueur, pas Bob (l'ancien voisin séquentiel d'Alice)
        var updated = _sut.CurrentSession!.Players;
        Assert.True(updated.Single(p => p.Name == "Charlie").IsFirstPlayer);
        Assert.False(updated.Single(p => p.Name == "Bob").IsFirstPlayer);
    }

    [Fact]
    public async Task CreateTeamAsync_AssignsSharedTeamIdToSelectedPlayersOnly()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");
        var bob = _sut.CurrentSession.Players.Single(p => p.Name == "Bob");

        // Act
        await _sut.CreateTeamAsync([alice.PlayerId, bob.PlayerId]);

        // Assert
        var players = _sut.CurrentSession.Players;
        var updatedAlice = players.Single(p => p.Name == "Alice");
        var updatedBob = players.Single(p => p.Name == "Bob");
        var charlie = players.Single(p => p.Name == "Charlie");

        Assert.NotNull(updatedAlice.TeamId);
        Assert.Equal(updatedAlice.TeamId, updatedBob.TeamId);
        Assert.Null(charlie.TeamId);
        Assert.Single(_sut.CurrentSession.Teams);
    }

    [Fact]
    public async Task AddPlayersToTeamAsync_AssignsUnassignedPlayersToExistingTeam()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        await _sut.AddPlayerToSetupAsync("Charlie", "🐰", "#0000FF");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");
        var bob = _sut.CurrentSession.Players.Single(p => p.Name == "Bob");
        var charlie = _sut.CurrentSession.Players.Single(p => p.Name == "Charlie");
        await _sut.CreateTeamAsync([alice.PlayerId, bob.PlayerId]);
        var teamId = _sut.CurrentSession.Players.Single(p => p.Name == "Alice").TeamId!.Value;

        // Act
        await _sut.AddPlayersToTeamAsync(teamId, [charlie.PlayerId]);

        // Assert
        var players = _sut.CurrentSession.Players;
        Assert.Equal(teamId, players.Single(p => p.Name == "Charlie").TeamId);
        Assert.Single(_sut.CurrentSession.Teams); // Pas de nouvelle équipe créée
    }

    [Fact]
    public async Task AddPlayersToTeamAsync_IgnoresPlayersAlreadyAssignedToAnotherTeam()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");
        var bob = _sut.CurrentSession.Players.Single(p => p.Name == "Bob");
        await _sut.CreateTeamAsync([alice.PlayerId]);
        await _sut.CreateTeamAsync([bob.PlayerId]);
        var aliceTeamId = _sut.CurrentSession.Players.Single(p => p.Name == "Alice").TeamId!.Value;
        var bobTeamId = _sut.CurrentSession.Players.Single(p => p.Name == "Bob").TeamId!.Value;

        // Act - Bob est déjà dans sa propre équipe, il doit être ignoré
        await _sut.AddPlayersToTeamAsync(aliceTeamId, [bob.PlayerId]);

        // Assert
        Assert.Equal(bobTeamId, _sut.CurrentSession.Players.Single(p => p.Name == "Bob").TeamId);
        Assert.Equal(2, _sut.CurrentSession.Teams.Length);
    }

    [Fact]
    public async Task AddPlayersToTeamAsync_WithUnknownTeamId_ShouldBeNoOp()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");

        // Act
        await _sut.AddPlayersToTeamAsync(Guid.NewGuid(), [alice.PlayerId]);

        // Assert
        Assert.Null(_sut.CurrentSession.Players.Single(p => p.Name == "Alice").TeamId);
        Assert.Empty(_sut.CurrentSession.Teams);
    }

    [Fact]
    public async Task RenameTeamAsync_UpdatesCustomNameForAllCurrentMembers()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId);
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].TeamId!.Value;

        // Act
        await _sut.RenameTeamAsync(teamId, "Nous");

        // Assert
        Assert.Equal("Nous", _sut.CurrentSession.Teams.Single(t => t.TeamId == teamId).CustomName);
    }

    [Fact]
    public async Task UpdateTeamEmojiAsync_UpdatesCustomEmojiForAllCurrentMembers()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId);
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].TeamId!.Value;

        // Act
        await _sut.UpdateTeamEmojiAsync(teamId, "🏆");

        // Assert
        Assert.Equal("🏆", _sut.CurrentSession.Teams.Single(t => t.TeamId == teamId).CustomEmoji);
    }

    [Fact]
    public async Task UpdatePlayerEmojiAsync_ChangesOnlyTheGivenPlayersEmoji()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");

        // Act
        await _sut.UpdatePlayerEmojiAsync(alice.PlayerId, "🦊");

        // Assert
        Assert.Equal("🦊", _sut.CurrentSession.Players.Single(p => p.Name == "Alice").Emoji);
        Assert.Equal("🐶", _sut.CurrentSession.Players.Single(p => p.Name == "Bob").Emoji);
    }

    [Fact]
    public async Task DeleteTeamAsync_ClearsTeamMembershipForAllMembers()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId);
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].TeamId!.Value;
        await _sut.RenameTeamAsync(teamId, "Nous");

        // Act
        await _sut.DeleteTeamAsync(teamId);

        // Assert
        Assert.All(_sut.CurrentSession.Players, p => Assert.Null(p.TeamId));
        Assert.Empty(_sut.CurrentSession.Teams);
    }

    [Fact]
    public async Task RemovePlayerFromTeamAsync_OnlyClearsTheGivenPlayer()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");
        var bob = _sut.CurrentSession.Players.Single(p => p.Name == "Bob");
        await _sut.CreateTeamAsync([alice.PlayerId, bob.PlayerId]);

        // Act
        await _sut.RemovePlayerFromTeamAsync(alice.PlayerId);

        // Assert
        Assert.Null(_sut.CurrentSession.Players.Single(p => p.Name == "Alice").TeamId);
        Assert.NotNull(_sut.CurrentSession.Players.Single(p => p.Name == "Bob").TeamId);
        Assert.Single(_sut.CurrentSession.Teams); // Bob reste dans l'équipe, elle survit
    }

    [Fact]
    public async Task RemovePlayerFromTeamAsync_WhenLastMemberLeaves_PrunesTheEmptyTeam()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        var alice = _sut.CurrentSession!.Players.Single(p => p.Name == "Alice");
        await _sut.CreateTeamAsync([alice.PlayerId]);

        // Act
        await _sut.RemovePlayerFromTeamAsync(alice.PlayerId);

        // Assert
        Assert.Null(_sut.CurrentSession.Players.Single(p => p.Name == "Alice").TeamId);
        Assert.Empty(_sut.CurrentSession.Teams);
    }

    [Fact]
    public async Task RecordTeamScoreAsync_WritesIdenticalScoreEntryToEveryMember()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId).ToList();
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].TeamId!.Value;
        await _sut.StartSessionAsync();

        // Act
        await _sut.RecordTeamScoreAsync(teamId, 1, 42);

        // Assert
        Assert.Equal(2, _sut.CurrentSession.Scores.Length);
        Assert.All(_sut.CurrentSession.Scores, s => Assert.Equal(42, s.Value));
        Assert.Equal(playerIds.ToHashSet(), _sut.CurrentSession.Scores.Select(s => s.PlayerId).ToHashSet());
    }

    [Fact]
    public async Task RecordTeamScoreAsync_ThenAdvanceToNextRoundAsync_BehavesLikeIndividualScoring()
    {
        // Arrange
        var rules = new GameRules(TargetScore: 50);
        var template = new GameTemplate(Guid.NewGuid(), "Team Game", ScoreType.Cumulative, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId).ToList();
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].TeamId!.Value;
        await _sut.StartSessionAsync();

        // Act - target score reached by the team's shared total
        await _sut.RecordTeamScoreAsync(teamId, 1, 60);
        await _sut.AdvanceToNextRoundAsync();

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
    }

    [Fact]
    public async Task FinishSessionAsync_ShouldSetStatusFinishedAndRecordEndedAt()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();

        // Act
        await _sut.FinishSessionAsync();

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
        Assert.NotNull(_sut.CurrentSession.EndedAt);
    }

    [Fact]
    public async Task FinishSessionAsync_WhenAlreadyFinished_ShouldBeNoOp()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        await _sut.FinishSessionAsync();
        var firstEndedAt = _sut.CurrentSession!.EndedAt;

        // Act
        await _sut.FinishSessionAsync();

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
        Assert.Equal(firstEndedAt, _sut.CurrentSession.EndedAt);
    }

    [Fact]
    public async Task AbandonSessionAsync_ShouldSetStatusAbandonedAndRecordEndedAt()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();

        // Act
        await _sut.AbandonSessionAsync();

        // Assert
        Assert.Equal(SessionStatus.Abandoned, _sut.CurrentSession!.Status);
        Assert.NotNull(_sut.CurrentSession.EndedAt);
    }

    [Fact]
    public async Task AbandonSessionAsync_WhenAlreadyAbandoned_ShouldBeNoOp()
    {
        // Arrange
        await _sut.InitializeNewSessionAsync(CreateDummyTemplate());
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        await _sut.AbandonSessionAsync();
        var firstEndedAt = _sut.CurrentSession!.EndedAt;

        // Act
        await _sut.AbandonSessionAsync();

        // Assert
        Assert.Equal(SessionStatus.Abandoned, _sut.CurrentSession!.Status);
        Assert.Equal(firstEndedAt, _sut.CurrentSession.EndedAt);
    }
}
