namespace Veloci.Data.Domain;

public class PilotAchievement
{
    public Guid Id { get; set; } = Guid.Empty;

    public virtual Pilot Pilot { get; set; }

    public DateTime Date { get; set; }

    public string Name { get; set; }
}
