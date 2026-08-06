using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class TeamNameFormatterTests
{
    [Fact]
    public void GetDisplayName_WhenNoCustomName_GeneratesNameFromMembersOrderedByDisplayOrder()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamId));
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 1, "#FF0000", TeamId: teamId),
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", TeamId: teamId),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, teams, players);

        // Assert
        Assert.Equal("David & Marion", result);
    }

    [Fact]
    public void GetDisplayName_WhenCustomNameSet_ReturnsCustomNameRegardlessOfMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamId, CustomName: "Nous"));
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 0, "#FF0000", TeamId: teamId),
            new(Guid.NewGuid(), "David", "🐶", 1, "#00FF00", TeamId: teamId),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, teams, players);

        // Assert
        Assert.Equal("Nous", result);
    }

    [Fact]
    public void GetDisplayName_IgnoresPlayersFromOtherTeams()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamId), new Team(otherTeamId));
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", TeamId: teamId),
            new(Guid.NewGuid(), "Alice", "🐱", 1, "#FF0000", TeamId: otherTeamId),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, teams, players);

        // Assert
        Assert.Equal("David", result);
    }
}
