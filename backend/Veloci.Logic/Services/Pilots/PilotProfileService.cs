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
    /// <summary>
    /// Returns the profile of the pilot, or <c>null</c> when no pilot goes by that name.
    /// </summary>
    Task<PilotProfileModel?> GetPilotProfileAsync(string pilotName, CancellationToken ct);
}

public class PilotProfileService : IPilotProfileService
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _allAchievements;
    private readonly RatingService _ratingService;
    private readonly ICupService _cupService;

    public PilotProfileService(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider,
        RatingService ratingService,
        ICupService cupService)
    {
        _pilots = pilots;
        _ratingService = ratingService;
        _cupService = cupService;
        _allAchievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task<PilotProfileModel?> GetPilotProfileAsync(string pilotName, CancellationToken ct)
    {
        var pilot = await _pilots.GetAll()
            .Include(p => p.DayStreakFreezes)
            .ByName(pilotName)
            .FirstOrDefaultAsync(ct);

        if (pilot is null)
            return null;

        var classRatings = new List<PilotClassRatingModel>();

        foreach (var cupId in _cupService.GetEnabledCupIds())
        {
            classRatings.Add(await BuildClassRatingAsync(pilot, cupId));
        }

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
            ClassRatings = classRatings
        };
    }

    private async Task<PilotClassRatingModel> BuildClassRatingAsync(Pilot pilot, string cupId)
    {
        var cupOptions = _cupService.GetCupOptions(cupId);

        var league = pilot.GetCurrentLeague(cupId)
            ?? (cupOptions.Leagues.Enabled ? cupOptions.Leagues.OthersName : null);

        return new PilotClassRatingModel
        {
            CupId = cupId,
            ClassName = cupOptions.Name,
            GlobalRating = await _ratingService.GetPilotRankAsync(cupId, pilot.Id),
            League = league,
            LeagueColor = string.IsNullOrEmpty(league)
                ? null
                : cupOptions.Leagues.Definitions.FirstOrDefault(x => x.Name == league)?.Color,
            RatingHistory = await _ratingService.GetPilotRatingHistoryAsync(cupId, pilot.Id)
        };
    }

    private static PilotAchievementModel CreatePilotAchievementModel(IAchievement achievement, Pilot pilot)
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
