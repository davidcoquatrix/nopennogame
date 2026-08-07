using System.Collections.Immutable;
using NPNG.Domain.Entities;
using NPNG.Domain.Services;

namespace NPNG.Tests.Domain.Services;

public class StructuredScoringDefinitionTests
{
    private readonly AkropolisScoringDefinition _sut = new();

    private static StructuredScoreDetail DetailWith(params (string Key, CategoryValue Value)[] values) =>
        new(values.Aggregate(ImmutableDictionary<string, CategoryValue>.Empty, (acc, kv) => acc.SetItem(kv.Key, kv.Value)));

    [Fact]
    public void CalculateTotal_SumsAllCategorySubtotalsPlusFreeValue()
    {
        // Arrange
        var detail = DetailWith(
            (AkropolisScoringDefinition.Keys.Carrieres, new CategoryValue(2, 3)),   // 6
            (AkropolisScoringDefinition.Keys.Habitations, new CategoryValue(1, 4)), // 4
            (AkropolisScoringDefinition.Keys.Marches, new CategoryValue(3, 2)),     // 6
            (AkropolisScoringDefinition.Keys.Casernes, new CategoryValue(0, 5)),    // 0
            (AkropolisScoringDefinition.Keys.Jardins, new CategoryValue(4, 1)),     // 4
            (AkropolisScoringDefinition.Keys.PierresRestantes, new CategoryValue(5)));

        // Act & Assert
        Assert.Equal(25, _sut.CalculateTotal(detail)); // 6+4+6+0+4+5
    }

    [Fact]
    public void CalculateTotal_MultiplierByCountCategory_IsMultiplicateurTimesQuartiers()
    {
        // Arrange
        var detail = DetailWith((AkropolisScoringDefinition.Keys.Carrieres, new CategoryValue(3, 4)));

        // Act & Assert
        Assert.Equal(12, _sut.CalculateTotal(detail));
    }

    [Fact]
    public void CalculateTotal_Empty_IsZero()
    {
        Assert.Equal(0, _sut.CalculateTotal(StructuredScoreDetail.Empty));
    }

    [Fact]
    public void SectionTotal_WithoutSectionGrouping_SumsAllCategories()
    {
        // Arrange: Akropolis categories don't declare a SectionKey, so they all belong to the null section
        var detail = DetailWith(
            (AkropolisScoringDefinition.Keys.Carrieres, new CategoryValue(2, 3)), // 6
            (AkropolisScoringDefinition.Keys.PierresRestantes, new CategoryValue(4)));

        // Act & Assert
        Assert.Equal(10, _sut.SectionTotal(null, detail));
    }

    [Fact]
    public void GetSectionBonus_DefaultImplementation_ReturnsNull()
    {
        Assert.Null(_sut.GetSectionBonus(null));
    }
}
