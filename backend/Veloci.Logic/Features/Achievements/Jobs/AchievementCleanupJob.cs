using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Jobs;

public class AchievementCleanupJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<AchievementCleanupJob>();

    private readonly IRepository<PilotAchievement> _pilotAchievements;
    private readonly IEnumerable<IAchievement> _achievements;

    public AchievementCleanupJob(IRepository<PilotAchievement> pilotAchievements, IEnumerable<IAchievement> achievements)
    {
        _pilotAchievements = pilotAchievements;
        _achievements = achievements;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var validNames = _achievements.Select(a => a.Name).ToList();

        var deleted = await _pilotAchievements
            .GetAll(pa => !validNames.Contains(pa.Name))
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            Log.Information("Deleted {Count} obsolete pilot achievements", deleted);
    }
}
