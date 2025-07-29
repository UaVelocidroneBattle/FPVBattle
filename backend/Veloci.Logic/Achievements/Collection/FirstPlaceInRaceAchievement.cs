using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

public class FirstPlaceInRaceAchievement : IAchievementAfterCompetition
{
    public string Name => "FirstPlaceInRace";
    public string Title => "Перший";
    public string Description => "Перше місце в гонці";

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return competition.IsPilotAtLocalRank(pilot, 1);
    }
}
