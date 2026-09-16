namespace Veloci.Logic.Features.Leagues;

public class PaceRatingSettings
{
    public const string SectionName = "PaceRating";

    public int MinDaysForRelevance { get; set; } = 7;
    public int TopPilotsForReference { get; set; } = 3;
    public LookBackPeriod LookBackPeriod { get; set; } = new(1, LookBackUnit.Month);
    public int DropWorstDaysCount { get; set; } = 1;
}