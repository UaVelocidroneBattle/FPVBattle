using Microsoft.EntityFrameworkCore;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Data.Achievements;

public class ThirdPlaceInRaceAchievement : IAchievementAfterCompetition
{
    private readonly IRepository<Competition> _competitions;

    public ThirdPlaceInRaceAchievement(IRepository<Competition> competitions)
    {
        _competitions = competitions;
    }

    public string Name => "ThirdPlaceInRace";
    public string Title => "Серед топів";
    public string Description => "Третє місце в гонці";
    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return competition.CompetitionResults
            .SingleOrDefault(res => res.LocalRank == 3)?.PlayerName == pilot.Name;
    }
}
