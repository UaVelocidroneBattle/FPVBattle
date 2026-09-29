using Hangfire;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Achievements.Jobs;

public class AchievementsJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<AchievementCleanupJob>(
            "Cleanup obsolete pilot achievements", x => x.ExecuteAsync(CancellationToken.None), Cron.Never());
    }
}
