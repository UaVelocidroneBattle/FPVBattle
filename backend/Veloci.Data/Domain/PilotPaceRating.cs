namespace Veloci.Data.Domain;

public class PilotPaceRating
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int PilotId { get; set; }
    public virtual Pilot Pilot { get; set; }

    public string CupId { get; set; }

    public double? AverageGapPercent { get; set; }

    public double? AverageGapChange { get; set; }

    public int Rank { get; set; }

    public int? RankChange { get; set; }

    public DateTime CalculatedOn { get; set; }
}
