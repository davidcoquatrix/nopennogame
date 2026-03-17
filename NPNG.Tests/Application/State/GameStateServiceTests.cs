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
}
