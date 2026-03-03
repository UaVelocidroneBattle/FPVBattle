using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class FirstInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "FirstInSeason_Whoop";
    public string Title => "Whoop Class Champion";
    public string Description => "Season winner (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(1)?.PlayerName == pilot.Name;
    }
}
