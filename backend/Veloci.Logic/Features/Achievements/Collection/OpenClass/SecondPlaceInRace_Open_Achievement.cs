using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.OpenClass;

public class SecondPlaceInRace_Open_Achievement : IAchievementAfterCompetition
{
    public string Name => "SecondPlaceInRace";
    public string Title => "Almost leader";
    public string Description => "Second place in a race (open class)";
    public string? CupId => CupIds.OpenClass;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return competition.IsPilotAtLocalRank(pilot, 2);
    }
}
