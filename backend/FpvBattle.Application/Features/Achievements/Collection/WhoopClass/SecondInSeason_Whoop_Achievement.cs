using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Achievements.Collection.WhoopClass;

public class SecondInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "SecondInSeason_Whoop";
    public string Title => "Silver Whoop";
    public string Description => "Second place in a season (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonLeaderboards.SelectMany(l => l.Results).GetByPlace(2)?.PlayerName == pilot.Name;
    }
}
