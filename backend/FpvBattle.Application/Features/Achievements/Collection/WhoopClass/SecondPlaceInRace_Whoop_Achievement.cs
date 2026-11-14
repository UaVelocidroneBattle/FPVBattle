using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Achievements.Collection.WhoopClass;

public class SecondPlaceInRace_Whoop_Achievement : IAchievementAfterCompetition
{
    public string Name => "SecondPlaceInRace_Whoop";
    public string Title => "Almost Whoop Leader";
    public string Description => "Second place in a race (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return competition.IsPilotAtLocalRank(pilot, 2);
    }
}
