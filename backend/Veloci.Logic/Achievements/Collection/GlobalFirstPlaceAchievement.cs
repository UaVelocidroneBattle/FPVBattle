using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

public class GlobalFirstPlaceAchievement : IAchievementAfterCompetition
{
    public string Name => "GlobalFirstPlace";
    public string Title => "GOAT";
    public string Description => "Перше місце в загальному лідерборді треку";
    public async Task<bool> CheckAsync(Pilot pilot, Competition competition)
    {
        if (pilot.HasAchievement(Name))
            return false;

        var result = competition.CompetitionResults.GetByPilotName(pilot.Name);

        if (result is null)
            throw new Exception("Result is null. Check the logic");

        return result.GlobalRank == 1;
    }
}
