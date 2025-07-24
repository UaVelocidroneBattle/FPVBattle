using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;

namespace Veloci.Data.Achievements;

public class ThirdInSeasonAchievement : IAchievementAfterSeason
{
    public string Name => "ThirdInSeason";
    public string Title => "Бронза";
    public string Description => "Третє місце в сезоні";

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return seasonResults.GetByPlace(3)?.PlayerName == pilot.Name;
    }
}
