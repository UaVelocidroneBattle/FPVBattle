using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection.OpenClass;

public class ThirdPlaceInRace_Open_Achievement : IAchievementAfterCompetition
{
    public string Name => "ThirdPlaceInRace";
    public string Title => "Among the best";
    public string Description => "Third place in a race (open class)";
    public string? CupId => CupIds.OpenClass;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return competition.IsPilotAtLocalRank(pilot, 3);
    }
}
