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
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 1, "#FF0000", Team: new TeamMembership(teamId)),
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", Team: new TeamMembership(teamId)),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, players);

        // Assert
        Assert.Equal("David & Marion", result);
    }

    [Fact]
    public void GetDisplayName_WhenCustomNameSet_ReturnsCustomNameRegardlessOfMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 0, "#FF0000", Team: new TeamMembership(teamId, CustomName: "Nous")),
            new(Guid.NewGuid(), "David", "🐶", 1, "#00FF00", Team: new TeamMembership(teamId, CustomName: "Nous")),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, players);

        // Assert
        Assert.Equal("Nous", result);
    }

    [Fact]
    public void GetDisplayName_IgnoresPlayersFromOtherTeams()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", Team: new TeamMembership(teamId)),
            new(Guid.NewGuid(), "Alice", "🐱", 1, "#FF0000", Team: new TeamMembership(otherTeamId)),
        };

        // Act
        var result = TeamNameFormatter.GetDisplayName(teamId, players);

        // Assert
        Assert.Equal("David", result);
    }
}
