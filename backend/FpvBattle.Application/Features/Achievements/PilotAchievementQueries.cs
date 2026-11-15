using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Achievements;

public static class PilotAchievementQueries
{
    public static IQueryable<PilotAchievement> ForPilot(this IQueryable<PilotAchievement> query, Pilot pilot) =>
        query.Where(a => a.PilotId == pilot.Id);

    public static IQueryable<PilotAchievement> FindByName(this IQueryable<PilotAchievement> query, string name)
    {
        return query.Where(pa => pa.Name == name);
    }
}
