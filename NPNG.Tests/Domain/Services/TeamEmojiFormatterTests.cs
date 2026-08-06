using System.Collections.Immutable;
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
        var teams = ImmutableArray.Create(new Team(teamId));

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, teams);

        // Assert
        Assert.Equal(TeamEmojiFormatter.DefaultEmoji, result);
    }

    [Fact]
    public void GetDisplayEmoji_WhenCustomEmojiSet_ReturnsCustomEmoji()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teams = ImmutableArray.Create(new Team(teamId, CustomEmoji: "🏆"));

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, teams);

        // Assert
        Assert.Equal("🏆", result);
    }

    [Fact]
    public void GetDisplayEmoji_IgnoresOtherTeams()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var otherTeamId = Guid.NewGuid();
        var teams = ImmutableArray.Create(
            new Team(teamId, CustomEmoji: "🏆"),
            new Team(otherTeamId, CustomEmoji: "🎲"));

        // Act
        var result = TeamEmojiFormatter.GetDisplayEmoji(teamId, teams);

        // Assert
        Assert.Equal("🏆", result);
    }
}
