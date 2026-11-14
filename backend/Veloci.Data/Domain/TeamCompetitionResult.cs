namespace Veloci.Data.Domain;

public class TeamCompetitionResult
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public virtual Team Team { get; set; }
    public string CompetitionId { get; set; }
    public virtual Competition Competition { get; set; }
    public int AverageTime { get; set; }
    public int Rank { get; set; }
    public int Points { get; set; }
}
