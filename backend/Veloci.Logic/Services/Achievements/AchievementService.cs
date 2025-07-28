using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services.Achievements;

public class AchievementService
{
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
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, competition);

            if (!triggered)
                continue;

            Log.Information("🏅 Pilot {PilotName} earned achievement {AchievementName}", pilot.Name, achievement.Name);
            pilot.AddAchievement(achievement);
            results.Add(new AchievementCheckResult(pilot, achievement));
        }

        Log.Debug("Achievement {AchievementName} check completed: {TriggeredCount} pilots earned it", 
            achievement.Name, results.Count);
        return results;
    }

    public async Task CheckAfterSeasonAsync(List<SeasonResult> results, CancellationToken cancellationToken)
    {
        Log.Information("Checking achievements after season completion for {PilotCount} pilots", results.Count);
        
        await CheckAndPublishAchievementsAsync<IAchievementAfterSeason>(
            achievement => CheckAchievementAfterSeason(achievement, results),
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterSeason(IAchievementAfterSeason achievement, List<SeasonResult> results)
    {
        var checkResults = new AchievementCheckResults();
        Log.Debug("Checking season achievement {AchievementName} for {PilotCount} pilots", 
            achievement.Name, results.Count);

        foreach (var result in results)
        {
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, results);

            if (!triggered)
                continue;

            Log.Information("🏆 Pilot {PilotName} earned season achievement {AchievementName}", pilot.Name, achievement.Name);
            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement));
        }

        Log.Debug("Season achievement {AchievementName} check completed: {TriggeredCount} pilots earned it", 
            achievement.Name, checkResults.Count);
        return checkResults;
    }

    public async Task CheckAfterTimeUpdateAsync(List<TrackTimeDelta> deltas, CancellationToken cancellationToken)
    {
        Log.Information("Checking achievements after time updates for {DeltaCount} result changes", deltas.Count);
        
        await CheckAndPublishAchievementsAsync<IAchievementAfterTimeUpdate>(
            achievement => CheckAchievementAfterTimeUpdate(achievement, deltas),
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterTimeUpdate(IAchievementAfterTimeUpdate achievement, List<TrackTimeDelta> deltas)
    {
        var checkResults = new AchievementCheckResults();
        Log.Debug("Checking time update achievement {AchievementName} for {DeltaCount} time changes", 
            achievement.Name, deltas.Count);

        foreach (var delta in deltas)
        {
            var pilot = await _pilots.FindAsync(delta.PlayerName);

            if (pilot is null) // Maybe a new pilot who is not in the DB yet
            {
                Log.Debug("Skipping achievement check for unknown pilot {PilotName}", delta.PlayerName);
                continue;
            }

            var triggered = await achievement.CheckAsync(pilot, deltas);

            if (!triggered)
                continue;

            Log.Information("⏱️ Pilot {PilotName} earned time-based achievement {AchievementName}", pilot.Name, achievement.Name);
            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement));
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
        CancellationToken cancellationToken
    ) where T : IAchievement
    {
        var achievements = GetAchievements<T>().ToList();
        Log.Debug("Processing {AchievementCount} achievements of type {AchievementType}", 
            achievements.Count, typeof(T).Name);
            
        var allResults = new AchievementCheckResults();

        foreach (var achievement in achievements)
        {
            var results = await processor(achievement);

            if (results.Any())
                allResults.AddRange(results);
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
}

public class AchievementCheckResults : List<AchievementCheckResult>
{ }

public class AchievementCheckResult(Pilot pilot, IAchievement achievement)
{
    public Pilot Pilot { get; set; } = pilot;
    public IAchievement Achievement { get; set; } = achievement;
}
