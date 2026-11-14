using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Leagues.Services;
using FpvBattle.Application.Features.Teams;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Services.Pilots.Models;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.Services.Pilots;

public interface IPilotProfileService
{
    /// <summary>
    /// Returns the profile of the pilot, or <c>null</c> when no pilot goes by that name.
    /// </summary>
    Task<PilotProfileModel?> GetPilotProfileAsync(string pilotName, CancellationToken ct);
}

public class PilotProfileService : IPilotProfileService
{
    private readonly IApplicationDbContext _db;
    private readonly TeamSettings _teamSettings;
    private readonly IEnumerable<IAchievement> _allAchievements;
    private readonly RatingService _ratingService;
    private readonly ICupService _cupService;

    public PilotProfileService(
        IApplicationDbContext db,
        IOptions<TeamSettings> teamSettings,
        IServiceProvider serviceProvider,
        RatingService ratingService,
        ICupService cupService)
    {
        _db = db;
        _teamSettings = teamSettings.Value;
        _ratingService = ratingService;
        _cupService = cupService;
        _allAchievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task<PilotProfileModel?> GetPilotProfileAsync(string pilotName, CancellationToken ct)
    {
        var pilot = await _db.Pilots
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
            ClassRatings = classRatings,
            Team = await GetTeamAsync(pilot, ct),
            IsPatron = await IsPatronAsync(pilot, ct)
        };
    }

    private Task<PilotTeamModel?> GetTeamAsync(Pilot pilot, CancellationToken ct)
        => _db.TeamMembers
            .Where(m => m.User.PilotId == pilot.Id && m.Team.Members.Count >= _teamSettings.MinPublicTeamSize)
            .Select(m => new PilotTeamModel { Id = m.TeamId, Name = m.Team.Name })
            .FirstOrDefaultAsync(ct);

    private Task<bool> IsPatronAsync(Pilot pilot, CancellationToken ct)
        => _db.PatreonSupporters
            .Paying()
            .AnyAsync(s => s.PilotId == pilot.Id, ct);

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
