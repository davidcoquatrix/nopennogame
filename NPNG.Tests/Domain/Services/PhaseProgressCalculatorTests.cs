using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class PhaseProgressCalculatorTests
{
    private readonly Guid _player1 = Guid.NewGuid();
    private readonly Guid _player2 = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();

    [Fact]
    public void GetCompletedPhaseCount_CountsOnlyCompletedEntriesForThatPlayer()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 5, PhaseDetail: new PhaseScoreDetail(1, true)),
            new(Guid.NewGuid(), _sessionId, _player1, 2, 8, PhaseDetail: new PhaseScoreDetail(2, false)), // ratée
            new(Guid.NewGuid(), _sessionId, _player1, 3, 2, PhaseDetail: new PhaseScoreDetail(2, true)),
            new(Guid.NewGuid(), _sessionId, _player2, 1, 5, PhaseDetail: new PhaseScoreDetail(1, true)),  // autre joueur
        };

        // Act
        var result = PhaseProgressCalculator.GetCompletedPhaseCount(_player1, entries);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public void GetCurrentPhase_ReturnsOneMoreThanCompletedCount()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 5, PhaseDetail: new PhaseScoreDetail(1, true)),
            new(Guid.NewGuid(), _sessionId, _player1, 2, 5, PhaseDetail: new PhaseScoreDetail(2, true)),
        };

        // Act
        var result = PhaseProgressCalculator.GetCurrentPhase(_player1, entries, totalPhases: 10);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void GetCurrentPhase_WhenAllPhasesCompleted_IsCappedAtTotalPhases()
    {
        // Arrange
        var entries = Enumerable.Range(1, 10)
            .Select(phase => new ScoreEntry(Guid.NewGuid(), _sessionId, _player1, phase, 0, PhaseDetail: new PhaseScoreDetail(phase, true)))
            .ToList();

        // Act
        var result = PhaseProgressCalculator.GetCurrentPhase(_player1, entries, totalPhases: 10);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void HasWon_WhenCompletedCountReachesWinningPhase_ReturnsTrue()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 0, PhaseDetail: new PhaseScoreDetail(1, true)),
            new(Guid.NewGuid(), _sessionId, _player1, 2, 0, PhaseDetail: new PhaseScoreDetail(2, true)),
        };

        // Act
        var result = PhaseProgressCalculator.HasWon(_player1, entries, winningPhase: 2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasWon_WhenCompletedCountBelowWinningPhase_ReturnsFalse()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 0, PhaseDetail: new PhaseScoreDetail(1, true)),
        };

        // Act
        var result = PhaseProgressCalculator.HasWon(_player1, entries, winningPhase: 10);

        // Assert
        Assert.False(result);
    }
}
