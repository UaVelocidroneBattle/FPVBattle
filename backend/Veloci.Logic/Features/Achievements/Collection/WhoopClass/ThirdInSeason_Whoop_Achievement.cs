using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class ThirdInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "ThirdInSeason_Whoop";
    public string Title => "Bronze whoop";
    public string Description => "Third place in a season (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(3)?.PlayerName == pilot.Name;
    }
}
