using FpvBattle.Domain;
using FpvBattle.Logic.Features.Achievements.Base;
using FpvBattle.Logic.Features.Cups;

namespace FpvBattle.Logic.Features.Achievements.Collection.WhoopClass;

public class FirstInSeason_Whoop_Achievement : IAchievementAfterSeason
{
    public string Name => "FirstInSeason_Whoop";
    public string Title => "Whoop Class Champion";
    public string Description => "Season winner (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, List<LeagueSeasonLeaderboard> seasonLeaderboards)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return seasonLeaderboards.SelectMany(l => l.Results).GetByPlace(1)?.PlayerName == pilot.Name;
    }
}
