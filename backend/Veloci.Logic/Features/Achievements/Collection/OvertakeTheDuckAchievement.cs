using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Services;

namespace Veloci.Logic.Features.Achievements.Collection;

public class OvertakeTheDuckAchievement : IAchievementAfterCompetition
{
    private readonly ModelsService _modelsService;

    public OvertakeTheDuckAchievement(ModelsService modelsService)
    {
        _modelsService = modelsService;
    }

    public string Name => "OvertakeTheDuck";
    public string Title => "Catch The Duck";
    public string Description => "Overtake StDuck on a track";
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

        var sameQuadClass = await _modelsService.QuadsTheSameClassAsync(pilotResult.ModelName, duckResult.ModelName);

        return pilotResult.LocalRank < duckResult.LocalRank && sameQuadClass;
    }
}
