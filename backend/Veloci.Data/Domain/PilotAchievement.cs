namespace Veloci.Data.Domain;

public class PilotAchievement
{
    public Guid Id { get; set; } = Guid.Empty;

    public virtual Pilot Pilot { get; set; }

    public int? PilotId { get; set; }

    public DateTime Date { get; set; }

    public string Name { get; set; }
}

public static class PilotAchievementQueries
{
    public static IQueryable<PilotAchievement> ForPilot(this IQueryable<PilotAchievement> query, Pilot pilot) =>
        query.Where(a => a.PilotId == pilot.Id);
}

public static class PilotAchievementExtensions
{
    public static IQueryable<PilotAchievement> FindByName(this IQueryable<PilotAchievement> query, string name)
    {
        return query.Where(pa => pa.Name == name);
    }
}
