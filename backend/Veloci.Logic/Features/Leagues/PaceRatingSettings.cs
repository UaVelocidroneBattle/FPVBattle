namespace Veloci.Logic.Features.Leagues;

public class PaceRatingSettings
{
    public const string SectionName = "PaceRating";

    public int MinDaysForRelevance { get; set; } = 7;
    public int TopPilotsForReference { get; set; } = 3;
    public int LookBackDays { get; set; } = 30;
}