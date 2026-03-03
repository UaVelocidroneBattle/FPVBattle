using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class OvertakeTheDuckAchievement : IAchievementAfterCompetition
{
    public string Name => "OvertakeTheDuck";
    public string Title => "Catch The Duck";
    public string Description => "Overtake the duck in a race";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        const int duckId = 70911;

        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        if (pilot.Id == duckId)
        {
            return false;
        }

        var duckResult = competition.CompetitionResults.GetByPilotId(duckId);
        var leaderboardMiddle = competition.GetSlowest()?.LocalRank / 2;

        if (duckResult is null || duckResult.LocalRank == 1 || duckResult.LocalRank > leaderboardMiddle)
        {
            return false;
        }

        var pilotResult = competition.CompetitionResults.GetByPilotId(pilot.Id);

        if (pilotResult is null)
        {
            return false;
        }

        return pilotResult.LocalRank < duckResult.LocalRank;
    }
}
