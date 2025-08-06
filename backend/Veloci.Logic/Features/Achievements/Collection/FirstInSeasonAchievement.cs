using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class FirstInSeasonAchievement : IAchievementAfterSeason
{
    public string Name => "FirstInSeason";
    public string Title => "Чемпіон";
    public string Description => "Переможець сезону";

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(1)?.PlayerName == pilot.Name;
    }
}
