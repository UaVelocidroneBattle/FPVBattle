namespace Veloci.Data.Domain;

public static class PilotAchievementExtensions
{
    public static IQueryable<PilotAchievement> FindByName(this IQueryable<PilotAchievement> query, string name)
    {
        return query.Where(pa => pa.Name == name);
    }
}
