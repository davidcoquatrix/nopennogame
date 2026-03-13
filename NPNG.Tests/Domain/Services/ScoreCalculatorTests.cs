using NPNG.Domain.Entities;
using NPNG.Domain.Enums;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class ScoreCalculatorTests
{
    private readonly Guid _player1 = Guid.NewGuid();
    private readonly Guid _player2 = Guid.NewGuid();
    private readonly Guid _player3 = Guid.NewGuid();
    private readonly Guid _sessionId = Guid.NewGuid();

    private List<Guid> PlayerIds => [_player1, _player2, _player3];

    [Fact]
    public void CalculateLeaderboard_WhenCumulative_HighestScoreWins()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 10),
            new(Guid.NewGuid(), _sessionId, _player1, 2, 20), // Total 30
            
            new(Guid.NewGuid(), _sessionId, _player2, 1, 50),
            new(Guid.NewGuid(), _sessionId, _player2, 2, 10), // Total 60 (Winner)
            
            new(Guid.NewGuid(), _sessionId, _player3, 1, 5),
            new(Guid.NewGuid(), _sessionId, _player3, 2, 5)   // Total 10 (Last)
        };

        // Act
        var result = ScoreCalculator.CalculateLeaderboard(ScoreType.Cumulative, PlayerIds, entries);

        // Assert
        Assert.Equal(3, result.Length);
        
        // Winner (Player 2)
        Assert.Equal(_player2, result[0].PlayerId);
        Assert.Equal(60, result[0].TotalScore);
        Assert.Equal(1, result[0].Rank);
        
        // Second (Player 1)
        Assert.Equal(_player1, result[1].PlayerId);
        Assert.Equal(30, result[1].TotalScore);
        Assert.Equal(2, result[1].Rank);
        
        // Last (Player 3)
        Assert.Equal(_player3, result[2].PlayerId);
        Assert.Equal(10, result[2].TotalScore);
        Assert.Equal(3, result[2].Rank);
    }

    [Fact]
    public void CalculateLeaderboard_WhenCumulativeLower_LowestScoreWins()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 10),
            new(Guid.NewGuid(), _sessionId, _player1, 2, 20), // Total 30
            
            new(Guid.NewGuid(), _sessionId, _player2, 1, 50),
            new(Guid.NewGuid(), _sessionId, _player2, 2, 10), // Total 60 (Last)
            
            new(Guid.NewGuid(), _sessionId, _player3, 1, 5),
            new(Guid.NewGuid(), _sessionId, _player3, 2, 5)   // Total 10 (Winner)
        };

        // Act
        var result = ScoreCalculator.CalculateLeaderboard(ScoreType.CumulativeLower, PlayerIds, entries);

        // Assert
        Assert.Equal(3, result.Length);
        
        // Winner (Player 3)
        Assert.Equal(_player3, result[0].PlayerId);
        Assert.Equal(10, result[0].TotalScore);
        Assert.Equal(1, result[0].Rank);
        
        // Second (Player 1)
        Assert.Equal(_player1, result[1].PlayerId);
        Assert.Equal(30, result[1].TotalScore);
        Assert.Equal(2, result[1].Rank);
        
        // Last (Player 2)
        Assert.Equal(_player2, result[2].PlayerId);
        Assert.Equal(60, result[2].TotalScore);
        Assert.Equal(3, result[2].Rank);
    }

    [Fact]
    public void CalculateLeaderboard_WhenTieOccurs_AssignsSameRankAndSkipsNext()
    {
        // Arrange
        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 50), // Total 50
            new(Guid.NewGuid(), _sessionId, _player2, 1, 50), // Total 50
            new(Guid.NewGuid(), _sessionId, _player3, 1, 20)  // Total 20
        };

        // Act
        var result = ScoreCalculator.CalculateLeaderboard(ScoreType.Cumulative, PlayerIds, entries);

        // Assert
        Assert.Equal(50, result[0].TotalScore);
        Assert.Equal(1, result[0].Rank); // Tied for 1st
        
        Assert.Equal(50, result[1].TotalScore);
        Assert.Equal(1, result[1].Rank); // Tied for 1st
        
        Assert.Equal(20, result[2].TotalScore);
        Assert.Equal(3, result[2].Rank); // 3rd (skipped 2nd)
    }
}
