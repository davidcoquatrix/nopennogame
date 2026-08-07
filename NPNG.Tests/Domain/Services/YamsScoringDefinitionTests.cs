using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class YamsScoringDefinitionTests
{
    private readonly YamsScoringDefinition _sut = new();

    private static StructuredScoreDetail DetailWith(params (string Key, CategoryValue Value)[] values) =>
        new(values.Aggregate(ImmutableDictionary<string, CategoryValue>.Empty, (acc, kv) => acc.SetItem(kv.Key, kv.Value)));

    [Fact]
    public void CalculateTotal_WhenUpperSectionBelowThreshold_NoBonusApplied()
    {
        // Arrange: section haute = 62 (< 63)
        var detail = DetailWith(
            (YamsScoringDefinition.Keys.As, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Deux, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Trois, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Quatre, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Cinq, new CategoryValue(11)),
            (YamsScoringDefinition.Keys.Six, new CategoryValue(11)));

        // Act & Assert
        Assert.Equal(62, _sut.CalculateTotal(detail));
    }

    [Fact]
    public void CalculateTotal_WhenUpperSectionReachesThreshold_BonusApplied()
    {
        // Arrange: section haute = 63 (>= 63)
        var detail = DetailWith(
            (YamsScoringDefinition.Keys.As, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Deux, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Trois, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Quatre, new CategoryValue(10)),
            (YamsScoringDefinition.Keys.Cinq, new CategoryValue(12)),
            (YamsScoringDefinition.Keys.Six, new CategoryValue(11)));

        // Act & Assert
        Assert.Equal(63 + YamsScoringDefinition.UpperSectionBonusPoints, _sut.CalculateTotal(detail));
    }

    [Theory]
    [InlineData(1, 25)]
    [InlineData(0, 0)]
    public void CalculateTotal_AchievedCategory_AddsFixedPointsOnlyWhenToggledOn(int achieved, int expectedTotal)
    {
        // Arrange
        var detail = DetailWith((YamsScoringDefinition.Keys.Full, new CategoryValue(achieved)));

        // Act & Assert
        Assert.Equal(expectedTotal, _sut.CalculateTotal(detail));
    }

    [Fact]
    public void CalculateTotal_LowerSectionFreeValueCategories_SumDirectly()
    {
        // Arrange
        var detail = DetailWith(
            (YamsScoringDefinition.Keys.Brelan, new CategoryValue(18)),
            (YamsScoringDefinition.Keys.Chance, new CategoryValue(20)));

        // Act & Assert
        Assert.Equal(38, _sut.CalculateTotal(detail));
    }

    [Fact]
    public void GetSectionBonus_ForUpperSection_ReturnsThresholdAndPoints()
    {
        // Arrange
        var upperSectionKey = _sut.Categories.First(c => c.Key == YamsScoringDefinition.Keys.As).SectionKey;

        // Act
        var bonus = _sut.GetSectionBonus(upperSectionKey);

        // Assert
        Assert.NotNull(bonus);
        Assert.Equal(YamsScoringDefinition.UpperSectionBonusThreshold, bonus!.Threshold);
        Assert.Equal(YamsScoringDefinition.UpperSectionBonusPoints, bonus.Points);
    }

    [Fact]
    public void GetSectionBonus_ForLowerSection_ReturnsNull()
    {
        // Arrange
        var lowerSectionKey = _sut.Categories.First(c => c.Key == YamsScoringDefinition.Keys.Chance).SectionKey;

        // Act & Assert
        Assert.Null(_sut.GetSectionBonus(lowerSectionKey));
    }
}
