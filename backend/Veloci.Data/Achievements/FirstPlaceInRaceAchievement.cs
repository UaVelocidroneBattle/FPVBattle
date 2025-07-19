using Microsoft.EntityFrameworkCore;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

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

        return competition.CompetitionResults
            .SingleOrDefault(res => res.LocalRank == 1)?.PlayerName == pilot.Name;
    }
}
