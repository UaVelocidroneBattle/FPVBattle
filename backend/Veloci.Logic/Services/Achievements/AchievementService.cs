using MediatR;
using Microsoft.Extensions.DependencyInjection;
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
        await CheckAndPublishAchievementsAsync<IAchievementAfterCompetition>(
            achievement => CheckAchievementAfterCompetition(achievement, competition),
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterCompetition(IAchievementAfterCompetition achievement, Competition competition)
    {
        var results = new AchievementCheckResults();

        foreach (var result in competition.CompetitionResults)
        {
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, competition);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
            results.Add(new AchievementCheckResult(pilot, achievement));
        }

        return results;
    }

    public async Task CheckAfterSeasonAsync(List<SeasonResult> results, CancellationToken cancellationToken)
    {
        await CheckAndPublishAchievementsAsync<IAchievementAfterSeason>(
            achievement => CheckAchievementAfterSeason(achievement, results),
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterSeason(IAchievementAfterSeason achievement, List<SeasonResult> results)
    {
        var checkResults = new AchievementCheckResults();

        foreach (var result in results)
        {
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, results);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement));
        }

        return checkResults;
    }

    public async Task CheckAfterTimeUpdateAsync(List<TrackTimeDelta> deltas, CancellationToken cancellationToken)
    {
        await CheckAndPublishAchievementsAsync<IAchievementAfterTimeUpdate>(
            achievement => CheckAchievementAfterTimeUpdate(achievement, deltas),
            cancellationToken
        );
    }

    private async Task<AchievementCheckResults> CheckAchievementAfterTimeUpdate(IAchievementAfterTimeUpdate achievement, List<TrackTimeDelta> deltas)
    {
        var checkResults = new AchievementCheckResults();

        foreach (var delta in deltas)
        {
            var pilot = await _pilots.FindAsync(delta.PlayerName);

            if (pilot is null) // Maybe a new pilot who is not in the DB yet
                continue;

            var triggered = await achievement.CheckAsync(pilot, deltas);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
            checkResults.Add(new AchievementCheckResult(pilot, achievement));
        }

        return checkResults;
    }

    public async Task CheckGlobalsAsync()
    {
        var achievements = GetAchievements<IGlobalAchievement>();

        foreach (var achievement in achievements)
        {
            await achievement.CheckAsync();
        }
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
        var achievements = GetAchievements<T>();
        var allResults = new AchievementCheckResults();

        foreach (var achievement in achievements)
        {
            var results = await processor(achievement);

            if (results.Any())
                allResults.AddRange(results);
        }

        await _pilots.SaveChangesAsync(cancellationToken);

        if (allResults.Any())
            await _mediator.Publish(new GotAchievements(allResults), cancellationToken);
    }
}

public class AchievementCheckResults : List<AchievementCheckResult>
{ }

public class AchievementCheckResult(Pilot pilot, IAchievement achievement)
{
    public Pilot Pilot { get; set; } = pilot;
    public IAchievement Achievement { get; set; } = achievement;
}
