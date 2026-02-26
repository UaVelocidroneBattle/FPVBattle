using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class GlobalFirstPlaceAchievement : IAchievementAfterCompetition
{
    public string Name => "GlobalFirstPlace";
    public string Title => "GOAT";
    public string Description => "First place on the track's global leaderboard";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var result = competition.CompetitionResults.GetByPilotId(pilot.Id);

        if (result is null)
        {
            throw new Exception("Result is null. Check the logic");
        }

        return result.GlobalRank == 1;
    }
}
