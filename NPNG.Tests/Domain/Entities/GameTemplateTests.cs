using NPNG.Domain.Entities;

namespace NPNG.Tests.Domain.Entities;

public class GameTemplateTests
{
    [Fact]
    public void TeamRules_RequiresEqualSizes_IsFalse_WithNoSizeConstraint()
    {
        // Arrange
        var rules = new TeamRules();

        // Act & Assert
        Assert.False(rules.RequiresEqualSizes);
    }

    [Fact]
    public void TeamRules_RequiresEqualSizes_IsTrue_WhenTeamSizeIsFixed()
    {
        // Arrange - une taille fixe impose de facto des équipes égales, même sans le flag explicite
        var rules = new TeamRules(TeamSize: 2);

        // Act & Assert
        Assert.True(rules.RequiresEqualSizes);
    }

    [Fact]
    public void TeamRules_RequiresEqualSizes_IsTrue_WhenExplicitFlagSetWithoutFixedSize()
    {
        // Arrange
        var rules = new TeamRules(RequireEqualTeams: true);

        // Act & Assert
        Assert.True(rules.RequiresEqualSizes);
    }
}
