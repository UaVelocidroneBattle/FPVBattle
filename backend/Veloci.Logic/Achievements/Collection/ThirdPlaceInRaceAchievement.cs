using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

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

        return competition.IsPilotAtLocalRank(pilot, 3);
    }
}
