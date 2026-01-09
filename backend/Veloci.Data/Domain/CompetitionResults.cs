namespace Veloci.Data.Domain;

public class CompetitionResults
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public virtual Competition Competition { get; set; }

    public string CompetitionId { get; set; }

    public virtual Pilot Pilot { get; set; }

    public int PilotId { get; set; }

    public int TrackTime { get; set; }

    public int LocalRank { get; set; }

    public int GlobalRank { get; set; }

    public int Points { get; set; }

    public string? ModelName { get; set; }
}
