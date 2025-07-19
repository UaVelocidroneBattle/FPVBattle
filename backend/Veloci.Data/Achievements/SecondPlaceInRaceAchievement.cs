using Microsoft.EntityFrameworkCore;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Data.Achievements;

public class SecondPlaceInRaceAchievement : IAchievementAfterCompetition
{
    private readonly IRepository<Competition> _competitions;

    public SecondPlaceInRaceAchievement(IRepository<Competition> competitions)
    {
        _competitions = competitions;
    }

    public string Name => "SecondPlaceInRace";
    public string Title => "Майже лідер";
    public string Description => "Друге місце в гонці";
    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return competition.CompetitionResults
            .SingleOrDefault(res => res.LocalRank == 2)?.PlayerName == pilot.Name;
    }
}
