using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class TeamEmojiFormatterTests
{
    [Fact]
    public void GetDisplayEmoji_WhenNoCustomEmoji_ReturnsDefaultHandshake()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 1, "#FF0000", Team: new TeamMembership(teamId)),
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", Team: new TeamMembership(teamId)),
        };

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, players);

        // Assert
        Assert.Equal(TeamEmojiFormatter.DefaultEmoji, result);
    }

    [Fact]
    public void GetDisplayEmoji_WhenCustomEmojiSet_ReturnsCustomEmojiRegardlessOfMembers()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "Marion", "🐱", 0, "#FF0000", Team: new TeamMembership(teamId, CustomEmoji: "🏆")),
            new(Guid.NewGuid(), "David", "🐶", 1, "#00FF00", Team: new TeamMembership(teamId, CustomEmoji: "🏆")),
        };

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, players);

        // Assert
        Assert.Equal("🏆", result);
    }

    [Fact]
    public void GetDisplayEmoji_IgnoresPlayersFromOtherTeams()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var players = new List<SessionPlayer>
        {
            new(Guid.NewGuid(), "David", "🐶", 0, "#00FF00", Team: new TeamMembership(teamId, CustomEmoji: "🏆")),
            new(Guid.NewGuid(), "Alice", "🐱", 1, "#FF0000", Team: new TeamMembership(otherTeamId, CustomEmoji: "🎲")),
        };

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, players);

        // Assert
        Assert.Equal("🏆", result);
    }
}
