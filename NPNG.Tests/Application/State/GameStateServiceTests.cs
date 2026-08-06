using Moq;
using NPNG.Application.Interfaces;
using NPNG.Application.State;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

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
        var template = new GameTemplate(Guid.NewGuid(), "Short Game", ScoreType.Cumulative, null, rules);
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

    [Fact]
    public async Task SubmitStructuredScoreAndFinishAsync_ShouldRecordDetailAtFixedRoundAndFinishSession()
    {
        // Arrange
        var template = new GameTemplate(Guid.NewGuid(), "Akropolis", ScoreType.Structured);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var aliceId = _sut.CurrentSession!.Players[0].PlayerId;

        var detail = new AkropolisScoreDetail(new(2, 3), new(0, 0), new(0, 0), new(0, 0), new(0, 0), 4); // Total 10

        // Act
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, AkropolisScoreDetail> { [aliceId] = detail });

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
        var entry = Assert.Single(_sut.CurrentSession.Scores);
        Assert.Equal(1, entry.Round);
        Assert.Equal(10, entry.Value);
        Assert.Equal(detail, entry.AkropolisDetail);
    }

    [Fact]
    public async Task ResumeSessionAsync_WhenStructured_KeepsCurrentRoundFixed()
    {
        // Arrange
        var template = new GameTemplate(Guid.NewGuid(), "Akropolis", ScoreType.Structured);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.StartSessionAsync();
        var aliceId = _sut.CurrentSession!.Players[0].PlayerId;

        var detail = new AkropolisScoreDetail(new(2, 3), new(0, 0), new(0, 0), new(0, 0), new(0, 0), 4); // Total 10
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, AkropolisScoreDetail> { [aliceId] = detail });

        // Act - re-open for editing, then re-submit a correction
        await _sut.ResumeSessionAsync();
        var correctedDetail = detail with { PierresRestantes = 9 }; // Total 15
        await _sut.SubmitStructuredScoreAndFinishAsync(new Dictionary<Guid, AkropolisScoreDetail> { [aliceId] = correctedDetail });

        // Assert - still a single round-1 entry, updated in place (no double-counting)
        Assert.Equal(1, _sut.CurrentSession!.CurrentRound);
        var entry = Assert.Single(_sut.CurrentSession.Scores);
        Assert.Equal(1, entry.Round);
        Assert.Equal(15, entry.Value);
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

        Assert.NotNull(updatedAlice.Team);
        Assert.Equal(updatedAlice.Team!.TeamId, updatedBob.Team!.TeamId);
        Assert.Null(charlie.Team);
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
        var teamId = _sut.CurrentSession.Players[0].Team!.TeamId;

        // Act
        await _sut.RenameTeamAsync(teamId, "Nous");

        // Assert
        Assert.All(_sut.CurrentSession.Players, p => Assert.Equal("Nous", p.Team?.CustomName));
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
        var teamId = _sut.CurrentSession.Players[0].Team!.TeamId;

        // Act
        await _sut.UpdateTeamEmojiAsync(teamId, "🏆");

        // Assert
        Assert.All(_sut.CurrentSession.Players, p => Assert.Equal("🏆", p.Team?.CustomEmoji));
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
        var teamId = _sut.CurrentSession.Players[0].Team!.TeamId;
        await _sut.RenameTeamAsync(teamId, "Nous");

        // Act
        await _sut.DeleteTeamAsync(teamId);

        // Assert
        Assert.All(_sut.CurrentSession.Players, p => Assert.Null(p.Team));
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
        Assert.Null(_sut.CurrentSession.Players.Single(p => p.Name == "Alice").Team);
        Assert.NotNull(_sut.CurrentSession.Players.Single(p => p.Name == "Bob").Team);
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
        var teamId = _sut.CurrentSession.Players[0].Team!.TeamId;
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
        var template = new GameTemplate(Guid.NewGuid(), "Team Game", ScoreType.Cumulative, null, rules);
        await _sut.InitializeNewSessionAsync(template);
        await _sut.AddPlayerToSetupAsync("Alice", "🐱", "#FF0000");
        await _sut.AddPlayerToSetupAsync("Bob", "🐶", "#00FF00");
        var playerIds = _sut.CurrentSession!.Players.Select(p => p.PlayerId).ToList();
        await _sut.CreateTeamAsync(playerIds);
        var teamId = _sut.CurrentSession.Players[0].Team!.TeamId;
        await _sut.StartSessionAsync();

        // Act - target score reached by the team's shared total
        await _sut.RecordTeamScoreAsync(teamId, 1, 60);
        await _sut.AdvanceToNextRoundAsync();

        // Assert
        Assert.Equal(SessionStatus.Finished, _sut.CurrentSession!.Status);
    }
}
