using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.OpenClass;

public class FirstInSeason_Open_Achievement : IAchievementAfterSeason
{
    public string Name => "FirstInSeason";
    public string Title => "Open Class Champion";
    public string Description => "Season winner (open class)";
    public string? CupId => CupIds.OpenClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<SeasonResult> seasonResults)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonResults.GetByPlace(1)?.PlayerName == pilot.Name;
    }
}
