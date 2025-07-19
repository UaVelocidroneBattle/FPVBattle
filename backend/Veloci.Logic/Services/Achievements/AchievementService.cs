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
        var achievements = _achievements.OfType<IAchievementAfterCompetition>().ToList();

        foreach (var achievement in achievements)
        {
            await CheckAchievementAfterCompetition(achievement, competition, cancellationToken);
        }

        await _pilots.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAchievementAfterCompetition(IAchievementAfterCompetition achievement, Competition competition, CancellationToken cancellationToken)
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
        var achievements = _achievements.OfType<IAchievementAfterSeason>().ToList();

        foreach (var achievement in achievements)
        {
            await CheckAchievementAfterSeason(achievement, results, cancellationToken);
        }

        await _pilots.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAchievementAfterSeason(IAchievementAfterSeason achievement, List<SeasonResult> results, CancellationToken cancellationToken)
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
        var achievements = _achievements.OfType<IAchievementAfterTimeUpdate>().ToList();

        foreach (var achievement in achievements)
        {
            await CheckAchievementAfterTimeUpdate(achievement, deltas, cancellationToken);
        }

        await _pilots.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckAchievementAfterTimeUpdate(IAchievementAfterTimeUpdate achievement, List<TrackTimeDelta> deltas, CancellationToken cancellationToken)
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

    public async Task CheckGlobalsAsync()
    {
        var achievements = _achievements.OfType<IGlobalAchievement>();

        foreach (var achievement in achievements)
        {
            await achievement.CheckAsync();
        }
    }
}
