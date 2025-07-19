using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;

namespace Veloci.Data.Achievements;

public class SecondInSeasonAchievement : IAchievementAfterSeason
{
    public string Name => "SecondInSeason";
    public string Title => "Срібло";
    public string Description => "Друге місце в сезоні";

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return seasonResults.SingleOrDefault(res => res.Rank == 2)?.PlayerName == pilot.Name;
    }
}
