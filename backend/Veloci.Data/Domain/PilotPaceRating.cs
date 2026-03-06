namespace Veloci.Data.Domain;

public class PilotPaceRating
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int PilotId { get; set; }
    public virtual Pilot Pilot { get; set; }

    public string CupId { get; set; }

    public double? AverageGapPercent { get; set; }

    // how changed AverageGapPercent compare to previous calculation
    public double? AverageGapChange { get; set; }

    // pilot's position in the rating
    public int Rank { get; set; }

    // how changed rank compare to previous calculation
    public int? RankChange { get; set; }

    public DateTime CalculatedOn { get; set; }
}
