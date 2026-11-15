namespace FpvBattle.Domain;

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
