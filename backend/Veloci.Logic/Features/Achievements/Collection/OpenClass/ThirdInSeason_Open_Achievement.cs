using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.OpenClass;

public class ThirdInSeason_Open_Achievement : IAchievementAfterSeason
{
    public string Name => "ThirdInSeason";
    public string Title => "Bronze";
    public string Description => "Third place in a season (open class)";
    public string? CupId => CupIds.OpenClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(3)?.PlayerName == pilot.Name;
    }
}
