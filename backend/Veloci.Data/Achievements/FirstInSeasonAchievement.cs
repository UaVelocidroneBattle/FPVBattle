using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;

namespace Veloci.Data.Achievements;

public class FirstInSeasonAchievement : IAchievementAfterSeason
{
    public string Name => "FirstInSeason";
    public string Title => "Чемпіон";
    public string Description => "Переможець сезону";

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return seasonResults.SingleOrDefault(res => res.Rank == 1)?.PlayerName == pilot.Name;
    }
}
