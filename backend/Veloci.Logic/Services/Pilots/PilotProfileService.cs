using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues.Services;
using Veloci.Logic.Services.Pilots.Models;

namespace Veloci.Logic.Services.Pilots;

public interface IPilotProfileService
{
    Task<PilotProfileModel> GetPilotProfileAsync(string pilotName, CancellationToken ct);
}

public class PilotProfileService : IPilotProfileService
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _allAchievements;
    private readonly RatingService _ratingService;

    public PilotProfileService(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider,
        RatingService ratingService)
    {
        _pilots = pilots;
        _ratingService = ratingService;
        _allAchievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task<PilotProfileModel> GetPilotProfileAsync(string pilotName, CancellationToken ct)
    {
        var pilot = await _pilots.GetAll()
            .Include(p => p.DayStreakFreezes)
            .ByName(pilotName)
            .SingleAsync(ct);

        return new PilotProfileModel
        {
            Name = pilot.Name,
            Country = pilot.Country,
            CurrentDayStreak = pilot.DayStreak,
            MaxDayStreak = pilot.MaxDayStreak,
            LastRaceDate = pilot.LastRaceDate,
            FirstRaceDate = pilot.CreatedAt,
            TotalRaceDays = pilot.TotalRaceDays,
            AvailableFreezes = pilot.DayStreakFreezeCount,
            Achievements = _allAchievements.Select(a => CreatePilotAchievementModel(a, pilot)).ToList(),
            GlobalRating = await _ratingService.GetPilotRankAsync(CupIds.OpenClass, pilot.Id),
            RatingHistory = await _ratingService.GetPilotRatingHistoryAsync(CupIds.OpenClass, pilot.Id)
        };
    }

    private PilotAchievementModel CreatePilotAchievementModel(IAchievement achievement, Pilot pilot)
    {
        var pa = pilot.Achievements.FirstOrDefault(a => a.Name == achievement.Name);

        return new PilotAchievementModel
        {
            Name = achievement.Name,
            AchievedOn = pa?.Date,
            Title = achievement.Title,
            Description = achievement.Description
        };
    }
}
