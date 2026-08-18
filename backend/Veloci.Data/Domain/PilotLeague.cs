namespace Veloci.Data.Domain;

public class PilotLeague
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int PilotId { get; set; }
    public virtual Pilot Pilot { get; set; }
    public required string CupId { get; set; }
    public DateTime Date { get; set; }
    public required string? League { get; set; }
    public LeagueRecordStatus Status { get; set; }
}

public enum LeagueRecordStatus
{
    Current = 1,
    Historical = 2
}

public static class PilotLeagueExtensions
{
    extension(IQueryable<PilotLeague> allRecords)
    {
        public IQueryable<PilotLeague> ForCup(string cupId)
        {
            return allRecords.Where(p => p.CupId == cupId);
        }

        public IQueryable<PilotLeague> ForPilot(int pilotId)
        {
            return allRecords.Where(p => p.PilotId == pilotId);
        }

        public IQueryable<PilotLeague> Active()
        {
            return allRecords.Where(p => p.Status == LeagueRecordStatus.Current);
        }
    }
}
