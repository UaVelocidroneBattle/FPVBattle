using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class SecondInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "SecondInSeason_Whoop";
    public string Title => "Silver whoop";
    public string Description => "Second place in a season (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(2)?.PlayerName == pilot.Name;
    }
}
