namespace Veloci.Data.Domain;

public class DayStreakFreeze
{
    public DayStreakFreeze()
    {

    }

    public DayStreakFreeze(DateTime today)
    {
        CreatedOn = today;
    }

    public Guid Id { get; set; } = Guid.Empty;
    public virtual Pilot Pilot { get; set; }
    public int? PilotId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? SpentOn { get; set; }
}
