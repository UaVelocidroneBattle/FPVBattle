using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class ThirdInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "ThirdInSeason_Whoop";
    public string Title => "Bronze Whoop";
    public string Description => "Third place in a season (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonLeaderboards.SelectMany(l => l.Results).GetByPlace(3)?.PlayerName == pilot.Name;
    }
}
