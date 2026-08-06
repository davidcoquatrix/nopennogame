using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Tests.Domain.Entities;

public class SessionTests
{
    [Fact]
    public void HasProgress_IsFalse_ForFreshSetupSession()
    {
        // Arrange
        var session = MakeSession(currentRound: 1, hasScore: false);

        // Act & Assert
        Assert.False(session.HasProgress);
    }

    [Fact]
    public void HasProgress_IsTrue_WhenPastRoundOne()
    {
        // Arrange
        var session = MakeSession(currentRound: 2, hasScore: false);

        // Act & Assert
        Assert.True(session.HasProgress);
    }

    [Fact]
    public void HasProgress_IsTrue_WhenRoundOneScoreAlreadyRecorded()
    {
        // Arrange
        var session = MakeSession(currentRound: 1, hasScore: true);

        // Act & Assert
        Assert.True(session.HasProgress);
    }

    private static Session MakeSession(int currentRound, bool hasScore)
    {
        var template = new GameTemplate(Guid.NewGuid(), "Test Game", ScoreType.Cumulative, null);
        var scores = hasScore
            ? ImmutableArray.Create(new ScoreEntry(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 1, 10))
            : ImmutableArray<ScoreEntry>.Empty;

        return new Session(
            Guid.NewGuid(),
            template,
            DateTime.UtcNow,
            ImmutableArray<SessionPlayer>.Empty,
            scores,
            ImmutableArray<Team>.Empty,
            currentRound);
    }
}
