using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.WhoopClass;

public class FirstPlaceInRace_Whoop_Achievement : IAchievementAfterCompetition
{
    public string Name => "FirstPlaceInRace_Whoop";
    public string Title => "Whoop track winner";
    public string Description => "First place in a race (whoop class)";
    public string? CupId => CupIds.WhoopClass;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return competition.IsPilotAtLocalRank(pilot, 1);
    }
}
