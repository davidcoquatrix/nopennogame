using NPNG.Domain.Entities;
using NPNG.Domain.Enums;

namespace NPNG.Domain.Services;

/// <summary>
/// Catégories de score du Yams : section haute (As à Six, valeur libre) avec bonus de 35 points si
/// son total atteint 63, et section basse (Brelan/Carré/Chance en valeur libre, Full/Petite Suite/
/// Grande Suite/Yams en valeur fixe officielle).
/// </summary>
public sealed class YamsScoringDefinition : StructuredScoringDefinition
{
    public static class Keys
    {
        public const string As = "As";
        public const string Deux = "Deux";
        public const string Trois = "Trois";
        public const string Quatre = "Quatre";
        public const string Cinq = "Cinq";
        public const string Six = "Six";
        public const string Brelan = "Brelan";
        public const string Carre = "Carre";
        public const string Full = "Full";
        public const string PetiteSuite = "PetiteSuite";
        public const string GrandeSuite = "GrandeSuite";
        public const string Yams = "Yams";
        public const string Chance = "Chance";
    }

    private const string SectionHaute = "Section haute";
    private const string SectionBasse = "Section basse";

    public const int UpperSectionBonusThreshold = 63;
    public const int UpperSectionBonusPoints = 35;

    private static readonly SectionBonus UpperSectionBonus = new(UpperSectionBonusThreshold, UpperSectionBonusPoints);

    public override StructuredGameKind Kind => StructuredGameKind.Yams;

    public override StructuredLayoutStyle LayoutStyle => StructuredLayoutStyle.Table;

    public override IReadOnlyList<CategoryDefinition> Categories { get; } =
    [
        new(Keys.As, "As", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Deux, "Deux", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Trois, "Trois", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Quatre, "Quatre", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Cinq, "Cinq", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Six, "Six", CategoryInputShape.FreeValue, SectionKey: SectionHaute, Icon: "🎲"),
        new(Keys.Brelan, "Brelan", CategoryInputShape.FreeValue, SectionKey: SectionBasse),
        new(Keys.Carre, "Carré", CategoryInputShape.FreeValue, SectionKey: SectionBasse),
        new(Keys.Full, "Full", CategoryInputShape.Achieved, FixedPoints: 25, SectionKey: SectionBasse),
        new(Keys.PetiteSuite, "Petite suite", CategoryInputShape.Achieved, FixedPoints: 30, SectionKey: SectionBasse),
        new(Keys.GrandeSuite, "Grande suite", CategoryInputShape.Achieved, FixedPoints: 40, SectionKey: SectionBasse),
        new(Keys.Yams, "Yams", CategoryInputShape.Achieved, FixedPoints: 50, SectionKey: SectionBasse),
        new(Keys.Chance, "Chance", CategoryInputShape.FreeValue, SectionKey: SectionBasse)
    ];

    public override int CalculateTotal(StructuredScoreDetail detail)
    {
        var baseTotal = base.CalculateTotal(detail);
        var upperSectionTotal = SectionTotal(SectionHaute, detail);
        var bonus = upperSectionTotal >= UpperSectionBonusThreshold ? UpperSectionBonusPoints : 0;

        return baseTotal + bonus;
    }

    public override SectionBonus? GetSectionBonus(string? sectionKey) =>
        sectionKey == SectionHaute ? UpperSectionBonus : null;
}
