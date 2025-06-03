namespace Veloci.Data.Domain;

public class DayStreakFreeze
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public virtual Pilot Pilot { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public DateTime? SpentOn { get; set; }
}
