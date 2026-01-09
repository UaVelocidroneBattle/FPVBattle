using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Services.Pilots.Models;

namespace Veloci.Logic.Services.Pilots;

public interface IPilotProfileService
{
    Task<PilotProfileModel> GetPilotProfileAsync(string pilotName, CancellationToken ct);
}

public class PilotProfileService : IPilotProfileService
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<CompetitionResults> _competitionResults;
    private readonly IRepository<PilotAchievement> _achievements;
    private readonly AchievementService _achievementService;

    public PilotProfileService(
        IRepository<Pilot> pilots,
        IRepository<CompetitionResults> competitionResults,
        IRepository<PilotAchievement> achievements,
        AchievementService achievementService)
    {
        _pilots = pilots;
        _competitionResults = competitionResults;
        _achievements = achievements;
        _achievementService = achievementService;
    }

    public async Task<PilotProfileModel> GetPilotProfileAsync(string pilotName, CancellationToken ct)
    {
        var pilot = await _pilots.GetAll()
            .Include(p => p.DayStreakFreezes)
            .ByName(pilotName)
            .SingleAsync(ct);

        // Calculate race statistics
        var raceDates = _competitionResults.GetAll()
            .Where(cr => cr.PilotId == pilot.Id && cr.Competition.State == CompetitionState.Closed)
            .Select(cr => cr.Competition.StartedOn.Date)
            .Distinct();

        var achievements = await _achievements.GetAll().ForPilot(pilot).ToListAsync(ct);

        return new PilotProfileModel
        {
            Name = pilot.Name,
            CurrentDayStreak = pilot.DayStreak,
            MaxDayStreak = pilot.MaxDayStreak,
            LastRaceDate = pilot.LastRaceDate,
            FirstRaceDate = await raceDates.MinAsync(ct),
            TotalRaceDays = await raceDates.CountAsync(ct),
            AvailableFreezes = pilot.DayStreakFreezeCount,
            Achievements = achievements.Select(CreatePilotAchievementModel).ToList()
        };
    }

    private PilotAchievementModel CreatePilotAchievementModel(PilotAchievement pa)
    {
        var achievement = _achievementService.GetAchievementByName(pa.Name);

        return new PilotAchievementModel
        {
            Name = pa.Name,
            EarnedOn = pa.Date,
            Title = achievement.Title,
            Description = achievement.Description
        };
    }
}
