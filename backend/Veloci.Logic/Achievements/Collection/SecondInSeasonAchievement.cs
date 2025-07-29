using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

public class SecondInSeasonAchievement : IAchievementAfterSeason
{
    public string Name => "SecondInSeason";
    public string Title => "Срібло";
    public string Description => "Друге місце в сезоні";

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return seasonResults.GetByPlace(2)?.PlayerName == pilot.Name;
    }
}
