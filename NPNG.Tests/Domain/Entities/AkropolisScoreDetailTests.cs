using NPNG.Domain.Entities;

namespace NPNG.Tests.Domain.Entities;

public class AkropolisScoreDetailTests
{
    [Fact]
    public void Total_SumsAllCategorySubtotalsPlusStones()
    {
        // Arrange
        var detail = new AkropolisScoreDetail(
            Carrieres: new(2, 3),   // 6
            Habitations: new(1, 4), // 4
            Marches: new(3, 2),     // 6
            Casernes: new(0, 5),    // 0
            Jardins: new(4, 1),     // 4
            PierresRestantes: 5);

        // Act & Assert
        Assert.Equal(25, detail.Total); // 6+4+6+0+4+5
    }

    [Fact]
    public void Subtotal_IsMultiplicateurTimesQuartiers()
    {
        // Arrange
        var category = new AkropolisCategoryScore(3, 4);

        // Act & Assert
        Assert.Equal(12, category.Subtotal);
    }

    [Fact]
    public void Empty_HasZeroTotal()
    {
        Assert.Equal(0, AkropolisScoreDetail.Empty.Total);
    }
}
