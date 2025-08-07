using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class LastInRaceAchievement : IAchievementAfterCompetition
{
    public string Name => "LastInRace";
    public string Title => "Перший з кінця";
    public string Description => "Останнє місце в гонці";

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        return competition.GetSlowest()?.PlayerName == pilot.Name;
    }
}
