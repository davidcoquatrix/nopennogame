using Moq;
using NPNG.Application.Interfaces;
using NPNG.Application.State;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Tests.Application.State;

public class SessionHistoryServiceTests
{
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly GameStateService _gameState;
    private readonly SessionHistoryService _sut; // System Under Test

    public SessionHistoryServiceTests()
    {
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _gameState = new GameStateService(_sessionRepositoryMock.Object);
        _sut = new SessionHistoryService(_sessionRepositoryMock.Object, _gameState);
    }

    private static Session CreateSession(SessionStatus status, DateTime startedAt)
    {
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative);
        return Session.Create(template) with { Status = status, StartedAt = startedAt };
    }

    [Fact]
    public async Task GetHistoryAsync_FiltersOutSetupAndActiveSessions_OrderedByMostRecentFirst()
    {
        // Arrange
        var oldFinished = CreateSession(SessionStatus.Finished, DateTime.UtcNow.AddDays(-2));
        var recentFinished = CreateSession(SessionStatus.Finished, DateTime.UtcNow.AddDays(-1));
        var setupSession = CreateSession(SessionStatus.Setup, DateTime.UtcNow);
        var activeSession = CreateSession(SessionStatus.Active, DateTime.UtcNow);

        _sessionRepositoryMock
            .Setup(repo => repo.GetAllSessionsAsync())
            .ReturnsAsync([oldFinished, recentFinished, setupSession, activeSession]);

        // Act
        var result = await _sut.GetHistoryAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(recentFinished.Id, result[0].Id);
        Assert.Equal(oldFinished.Id, result[1].Id);
    }

    [Fact]
    public async Task GetSessionByIdAsync_DelegatesToRepository()
    {
        // Arrange
        var session = CreateSession(SessionStatus.Finished, DateTime.UtcNow);
        _sessionRepositoryMock
            .Setup(repo => repo.GetSessionAsync(session.Id))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetSessionByIdAsync(session.Id);

        // Assert
        Assert.Equal(session.Id, result?.Id);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenDeletingCurrentSession_ClearsCurrentSessionInGameState()
    {
        // Arrange
        await _gameState.InitializeNewSessionAsync(new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative));
        var currentSessionId = _gameState.CurrentSession!.Id;

        // Act
        await _sut.DeleteSessionAsync(currentSessionId);

        // Assert
        Assert.Null(_gameState.CurrentSession);
        _sessionRepositoryMock.Verify(repo => repo.DeleteSessionAsync(currentSessionId), Times.Once);
    }

    [Fact]
    public async Task DeleteSessionAsync_WhenDeletingAnotherSession_LeavesCurrentSessionUntouched()
    {
        // Arrange
        await _gameState.InitializeNewSessionAsync(new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative));
        var currentSessionId = _gameState.CurrentSession!.Id;
        var otherSessionId = Guid.NewGuid();

        // Act
        await _sut.DeleteSessionAsync(otherSessionId);

        // Assert
        Assert.NotNull(_gameState.CurrentSession);
        Assert.Equal(currentSessionId, _gameState.CurrentSession.Id);
        _sessionRepositoryMock.Verify(repo => repo.DeleteSessionAsync(otherSessionId), Times.Once);
    }
}
