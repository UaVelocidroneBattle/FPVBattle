namespace Veloci.Data.Domain;

public class LeagueHistoryRecord
{
    public Guid Id { get; set; } = Guid.Empty;
    public virtual League? OldLeague { get; set; }
    public virtual League NewLeague { get; set; }
    public DateTime Date { get; set; }
}
