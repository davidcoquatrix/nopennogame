using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Enums;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class ScoreCalculatorTests
{
    private readonly Guid _player1 = Guid.NewGuid();
    private readonly Guid _player2 = Guid.NewGuid();
    private readonly Guid _player3 = Guid.NewGuid();
    private readonly Guid _player4 = Guid.NewGuid();
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

    [Fact]
    public void CalculateLeaderboard_WhenStructured_HighestScoreWinsLikeCumulative()
    {
        // Arrange
        var definition = new AkropolisScoringDefinition();
        var detail = new StructuredScoreDetail(ImmutableDictionary<string, CategoryValue>.Empty
            .Add(AkropolisScoringDefinition.Keys.Carrieres, new CategoryValue(2, 3)) // Subtotal 6
            .Add(AkropolisScoringDefinition.Keys.PierresRestantes, new CategoryValue(4))); // Total 10

        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, definition.CalculateTotal(detail), detail), // 10
            new(Guid.NewGuid(), _sessionId, _player2, 1, 25),                    // 25 (Winner)
            new(Guid.NewGuid(), _sessionId, _player3, 1, 15)                     // 15
        };

        // Act
        var result = ScoreCalculator.CalculateLeaderboard(ScoreType.Structured, PlayerIds, entries);

        // Assert
        Assert.Equal(_player2, result[0].PlayerId);
        Assert.Equal(25, result[0].TotalScore);
        Assert.Equal(1, result[0].Rank);

        Assert.Equal(_player3, result[1].PlayerId);
        Assert.Equal(15, result[1].TotalScore);
        Assert.Equal(2, result[1].Rank);

        Assert.Equal(_player1, result[2].PlayerId);
        Assert.Equal(10, result[2].TotalScore);
        Assert.Equal(3, result[2].Rank);
    }

    [Fact]
    public void GroupIntoTeams_TwoTeamsOfTwo_AssignsSequentialTeamRanks()
    {
        // Arrange
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamA), new Team(teamB));
        var players = ImmutableArray.Create(
            new SessionPlayer(_player1, "P1", "🐱", 0, "#fff", TeamId: teamA),
            new SessionPlayer(_player2, "P2", "🐶", 1, "#fff", TeamId: teamA),
            new SessionPlayer(_player3, "P3", "🐰", 2, "#fff", TeamId: teamB),
            new SessionPlayer(_player4, "P4", "🦊", 3, "#fff", TeamId: teamB));

        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 30),
            new(Guid.NewGuid(), _sessionId, _player2, 1, 30),
            new(Guid.NewGuid(), _sessionId, _player3, 1, 50),
            new(Guid.NewGuid(), _sessionId, _player4, 1, 50),
        };

        var leaderboard = ScoreCalculator.CalculateLeaderboard(ScoreType.Cumulative, players.Select(p => p.PlayerId), entries);

        // Act
        var result = ScoreCalculator.GroupIntoTeams(leaderboard, players, teams);

        // Assert
        Assert.Equal(2, result.Length);

        var winner = result.Single(t => t.TeamId == teamB);
        Assert.Equal(50, winner.TotalScore);
        Assert.Equal(1, winner.Rank);
        Assert.Equal(2, winner.MemberPlayerIds.Length);

        var second = result.Single(t => t.TeamId == teamA);
        Assert.Equal(30, second.TotalScore);
        Assert.Equal(2, second.Rank); // pas 3, malgré les 2 individus devant elle
    }

    [Fact]
    public void GroupIntoTeams_UnevenSplit_GroupsCorrectlyRegardlessOfTeamSize()
    {
        // Arrange
        var soloTeam = Guid.NewGuid();
        var duoTeam = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(soloTeam), new Team(duoTeam));
        var players = ImmutableArray.Create(
            new SessionPlayer(_player1, "P1", "🐱", 0, "#fff", TeamId: soloTeam),
            new SessionPlayer(_player2, "P2", "🐶", 1, "#fff", TeamId: duoTeam),
            new SessionPlayer(_player3, "P3", "🐰", 2, "#fff", TeamId: duoTeam));

        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 40),
            new(Guid.NewGuid(), _sessionId, _player2, 1, 20),
            new(Guid.NewGuid(), _sessionId, _player3, 1, 20),
        };

        var leaderboard = ScoreCalculator.CalculateLeaderboard(ScoreType.Cumulative, players.Select(p => p.PlayerId), entries);

        // Act
        var result = ScoreCalculator.GroupIntoTeams(leaderboard, players, teams);

        // Assert
        Assert.Equal(2, result.Length);

        var solo = result.Single(t => t.TeamId == soloTeam);
        Assert.Single(solo.MemberPlayerIds);
        Assert.Equal(1, solo.Rank);

        var duo = result.Single(t => t.TeamId == duoTeam);
        Assert.Equal(2, duo.MemberPlayerIds.Length);
        Assert.Equal(2, duo.Rank);
    }

    [Fact]
    public void GroupIntoTeams_WhenTwoTeamsTie_AssignsSameRank()
    {
        // Arrange
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamA), new Team(teamB));
        var players = ImmutableArray.Create(
            new SessionPlayer(_player1, "P1", "🐱", 0, "#fff", TeamId: teamA),
            new SessionPlayer(_player2, "P2", "🐶", 1, "#fff", TeamId: teamB));

        var entries = new List<ScoreEntry>
        {
            new(Guid.NewGuid(), _sessionId, _player1, 1, 30),
            new(Guid.NewGuid(), _sessionId, _player2, 1, 30),
        };

        var leaderboard = ScoreCalculator.CalculateLeaderboard(ScoreType.Cumulative, players.Select(p => p.PlayerId), entries);

        // Act
        var result = ScoreCalculator.GroupIntoTeams(leaderboard, players, teams);

        // Assert
        Assert.All(result, t => Assert.Equal(1, t.Rank));
    }
}
