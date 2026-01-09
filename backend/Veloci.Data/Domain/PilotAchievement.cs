namespace Veloci.Data.Domain;

public class PilotAchievement
{
    public Guid Id { get; set; } = Guid.Empty;

    public virtual Pilot Pilot { get; set; }

    public int? PilotId { get; set; }

    public DateTime Date { get; set; }

    public string Name { get; set; }
}

public static class PilotAchievemntQueries
{
    public static IQueryable<PilotAchievement> ForPilot(this IQueryable<PilotAchievement> query, Pilot pilot) =>
        query.Where(a => a.PilotId == pilot.Id);
}
