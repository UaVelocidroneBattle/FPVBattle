using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;

namespace Veloci.Data.Achievements;

public class LastInRaceAchievement : IAchievementAfterCompetition
{
    public string Name => "LastInRace";
    public string Title => "Перший з кінця";
    public string Description => "Останнє місце в гонці";

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return competition.CompetitionResults
            .OrderBy(res => res.LocalRank)
            .LastOrDefault()?.PlayerName == pilot.Name;
    }
}
