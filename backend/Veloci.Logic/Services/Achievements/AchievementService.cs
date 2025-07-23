using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services.Achievements;

public class AchievementService
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _achievements;

    public AchievementService(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider)
    {
        _pilots = pilots;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task CheckAfterCompetitionAsync(Competition competition, CancellationToken cancellationToken)
    {
        await ProcessAchievementsAndSaveAsync<IAchievementAfterCompetition>(
            achievement => CheckAchievementAfterCompetition(achievement, competition),
            cancellationToken);
    }

    private async Task CheckAchievementAfterCompetition(IAchievementAfterCompetition achievement, Competition competition)
    {
        foreach (var result in competition.CompetitionResults)
        {
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, competition);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
        }
    }

    public async Task CheckAfterSeasonAsync(List<SeasonResult> results, CancellationToken cancellationToken)
    {
        await ProcessAchievementsAndSaveAsync<IAchievementAfterSeason>(
            achievement => CheckAchievementAfterSeason(achievement, results),
            cancellationToken);
    }

    private async Task CheckAchievementAfterSeason(IAchievementAfterSeason achievement, List<SeasonResult> results)
    {
        foreach (var result in results)
        {
            var pilot = await _pilots.FindAsync(result.PlayerName);

            if (pilot is null)
                throw new Exception("Pilot not found");

            var triggered = await achievement.CheckAsync(pilot, results);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
        }
    }

    public async Task CheckAfterTimeUpdateAsync(List<TrackTimeDelta> deltas, CancellationToken cancellationToken)
    {
        await ProcessAchievementsAndSaveAsync<IAchievementAfterTimeUpdate>(
            achievement => CheckAchievementAfterTimeUpdate(achievement, deltas),
            cancellationToken);
    }

    private async Task CheckAchievementAfterTimeUpdate(IAchievementAfterTimeUpdate achievement, List<TrackTimeDelta> deltas)
    {
        foreach (var delta in deltas)
        {
            var pilot = await _pilots.FindAsync(delta.PlayerName);

            if (pilot is null) // Maybe a new pilot who is not in the DB yet
                continue;

            var triggered = await achievement.CheckAsync(pilot, deltas);

            if (!triggered)
                continue;

            pilot.AddAchievement(achievement);
        }
    }

    public async Task CheckGlobalsAsync(CancellationToken cancellationToken = default)
    {
        await ProcessAchievementsAndSaveAsync<IGlobalAchievement>(
            achievement => achievement.CheckAsync(),
            cancellationToken);
    }

    private IEnumerable<T> GetAchievements<T>() where T : IAchievement
    {
        return _achievements.OfType<T>();
    }

    private async Task ProcessAchievementsAndSaveAsync<T>(
        Func<T, Task> processor,
        CancellationToken cancellationToken) where T : IAchievement
    {
        foreach (var achievement in GetAchievements<T>())
        {
            await processor(achievement);
        }
        await _pilots.SaveChangesAsync(cancellationToken);
    }
}
