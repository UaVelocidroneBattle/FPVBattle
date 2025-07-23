using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;

namespace Veloci.Data.Achievements;

public class FirstPlaceInRaceAchievement : IAchievementAfterCompetition
{
    public string Name => "FirstPlaceInRace";
    public string Title => "Перший";
    public string Description => "Перше місце в гонці";

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return competition.GetWinner()?.PlayerName == pilot.Name;
    }
}
