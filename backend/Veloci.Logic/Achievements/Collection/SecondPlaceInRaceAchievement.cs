using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

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

        return competition.IsPilotAtLocalRank(pilot, 2);
    }
}
