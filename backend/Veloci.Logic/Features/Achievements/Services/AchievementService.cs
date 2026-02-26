using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Achievements.Notifications;

namespace Veloci.Logic.Features.Achievements.Services;

public class AchievementService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AchievementService>();

    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _achievements;
    private readonly IMediator _mediator;

    public AchievementService(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider,
        IMediator mediator)
    {
        _pilots = pilots;
        _mediator = mediator;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task CheckAfterCompetitionAsync(Competition competition, CancellationToken cancellationToken)
    {
        Log.Information("Checking achievements after competition {CompetitionId} for track {TrackName}",
            competition.Id, competition.Track.Name);

        await CheckAndPublishAchievementsAsync<IAchievementAfterCompetition>(
            achievement => CheckAchievementAfterCompetition(achievement, competition),
            competition.CupId,
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterCompetition(IAchievementAfterCompetition achievement, Competition competition)
    {
        var results = new AchievementCheckResults();
        Log.Debug("Checking achievement {AchievementName} for {PilotCount} pilots",
            achievement.Name, competition.CompetitionResults.Count);

        foreach (var result in competition.CompetitionResults)
        {
            var pilot = result.Pilot;
            var triggered = await achievement.CheckAsync(pilot, competition);

            if (!triggered)
            {
                continue;
            }

            Log.Information("🏅 Pilot {PilotName} earned achievement {AchievementName}", pilot.Name, achievement.Name);
            pilot.AddAchievement(achievement);
            results.Add(new AchievementCheckResult(pilot, achievement, competition.CupId));
        }

        Log.Debug("Achievement {AchievementName} check completed: {TriggeredCount} pilots earned it", achievement.Name, results.Count);
        return results;
    }

    public async Task CheckAfterSeasonAsync(List<SeasonResult> results, string cupId, CancellationToken cancellationToken)
    {
        Log.Information("Checking achievements after season completion for cup {CupId} with {PilotCount} pilots", cupId, results.Count);

        await CheckAndPublishAchievementsAsync<IAchievementAfterSeason>(
            achievement => CheckAchievementAfterSeason(achievement, results, cupId),
            cupId,
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterSeason(IAchievementAfterSeason achievement,
        List<SeasonResult> results, string cupId)
    {
        var checkResults = new AchievementCheckResults();
        Log.Debug("Checking season achievement {AchievementName} for cup {CupId} with {PilotCount} pilots",
            achievement.Name, cupId, results.Count);

        foreach (var result in results)
        {
            var pilot = await _pilots.GetAll().ByName(result.PlayerName).FirstOrDefaultAsync();

            if (pilot is null)
            {
                throw new Exception("Pilot not found");
            }

            var triggered = await achievement.CheckAsync(pilot, results);

            if (!triggered)
            {
                continue;
            }

            Log.Information("🏆 Pilot {PilotName} earned season achievement {AchievementName} in cup {CupId}", pilot.Name,
                achievement.Name, cupId);
            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement, cupId));
        }

        Log.Debug("Season achievement {AchievementName} check completed for cup {CupId}: {TriggeredCount} pilots earned it",
            achievement.Name, cupId, checkResults.Count);
        return checkResults;
    }

    public async Task CheckAfterTimeUpdateAsync(Competition competition, List<TrackTimeDelta> deltas, CancellationToken cancellationToken)
    {
        Log.Information("Checking achievements after time updates for {DeltaCount} result changes", deltas.Count);

        await CheckAndPublishAchievementsAsync<IAchievementAfterTimeUpdate>(
            achievement => CheckAchievementAfterTimeUpdate(achievement, deltas, competition.CupId),
            competition.CupId,
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterTimeUpdate(IAchievementAfterTimeUpdate achievement,
        List<TrackTimeDelta> deltas, string cupId)
    {
        var checkResults = new AchievementCheckResults();
        Log.Debug("Checking time update achievement {AchievementName} for {DeltaCount} time changes",
            achievement.Name, deltas.Count);

        foreach (var delta in deltas)
        {
            var pilot = delta.Pilot;

            if (pilot is null)
            {
                throw new Exception("Pilot not found");
            }

            var triggered = await achievement.CheckAsync(pilot, deltas);

            if (!triggered)
            {
                continue;
            }

            Log.Information("⏱️ Pilot {PilotName} earned time-based achievement {AchievementName}", pilot.Name,
                achievement.Name);
            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement, cupId));
        }

        Log.Debug("Time update achievement {AchievementName} check completed: {TriggeredCount} pilots earned it",
            achievement.Name, checkResults.Count);
        return checkResults;
    }

    public async Task CheckGlobalsAsync()
    {
        var achievements = GetAchievements<IGlobalAchievement>().ToList();
        Log.Information("Checking {GlobalAchievementCount} global achievements", achievements.Count);

        foreach (var achievement in achievements)
        {
            Log.Debug("Checking global achievement {AchievementName}", achievement.Name);
            await achievement.CheckAsync();
        }

        Log.Debug("Global achievement check completed");
    }

    private IEnumerable<T> GetAchievements<T>() where T : IAchievement
    {
        return _achievements.OfType<T>();
    }

    private async Task CheckAndPublishAchievementsAsync<T>(
        Func<T, Task<AchievementCheckResults>> processor,
        string cupId,
        CancellationToken cancellationToken
    ) where T : IAchievement
    {
        var achievements = GetAchievements<T>()
            .Where(a => a.CupId == cupId || a.CupId == null)
            .ToList();

        Log.Debug("Processing {AchievementCount} achievements of type {AchievementType}", achievements.Count, typeof(T).Name);

        var allResults = new AchievementCheckResults();

        foreach (var achievement in achievements)
        {
            var results = await processor(achievement);

            if (results.Any())
            {
                allResults.AddRange(results);
            }
        }

        await _pilots.SaveChangesAsync(cancellationToken);

        if (allResults.Any())
        {
            var uniquePilots = allResults.Select(r => r.Pilot.Name).Distinct().Count();
            Log.Information("Achievement check completed: {TriggeredCount} achievements awarded to {PilotCount} pilots",
                allResults.Count, uniquePilots);
            await _mediator.Publish(new GotAchievements(allResults), cancellationToken);
        }
        else
        {
            Log.Debug("No achievements were triggered in this check");
        }
    }

    public IAchievement GetAchievementByName(string name)
    {
        return _achievements.First(a => a.Name == name);
    }
}

public class AchievementCheckResults : List<AchievementCheckResult>
{
}

public class AchievementCheckResult(Pilot pilot, IAchievement achievement, string? cupId = null)
{
    public Pilot Pilot { get; set; } = pilot;
    public IAchievement Achievement { get; set; } = achievement;
    public string? CupId { get; set; } = cupId;
}
