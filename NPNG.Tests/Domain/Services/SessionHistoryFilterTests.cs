using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class SessionHistoryFilterTests
{
    private static Session CreateSession(SessionStatus status, int scoreCount)
    {
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative);
        var session = Session.Create(template) with { Status = status };

        if (scoreCount == 0)
        {
            return session;
        }

        var playerId = Guid.NewGuid();
        var scores = Enumerable.Range(1, scoreCount)
            .Select(round => new ScoreEntry(Guid.NewGuid(), session.Id, playerId, round, 10))
            .ToImmutableArray();

        return session with { Scores = scores };
    }

    [Fact]
    public void IsRelevantForHistory_WhenFinished_ReturnsTrue()
    {
        // Arrange
        var session = CreateSession(SessionStatus.Finished, scoreCount: 0);

        // Act
        var result = SessionHistoryFilter.IsRelevantForHistory(session);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRelevantForHistory_WhenAbandonedWithScores_ReturnsTrue()
    {
        // Arrange
        var session = CreateSession(SessionStatus.Abandoned, scoreCount: 1);

        // Act
        var result = SessionHistoryFilter.IsRelevantForHistory(session);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsRelevantForHistory_WhenAbandonedWithoutScores_ReturnsFalse()
    {
        // Arrange
        var session = CreateSession(SessionStatus.Abandoned, scoreCount: 0);

        // Act
        var result = SessionHistoryFilter.IsRelevantForHistory(session);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(SessionStatus.Setup)]
    [InlineData(SessionStatus.Active)]
    public void IsRelevantForHistory_WhenSetupOrActive_ReturnsFalse(SessionStatus status)
    {
        // Arrange
        var session = CreateSession(status, scoreCount: 1);

        // Act
        var result = SessionHistoryFilter.IsRelevantForHistory(session);

        // Assert
        Assert.False(result);
    }
}
