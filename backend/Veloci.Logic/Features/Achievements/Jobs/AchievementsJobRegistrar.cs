using Hangfire;
using Veloci.Logic.Features.Achievements.Jobs;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Achievements.Jobs;

public class AchievementsJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<DayStreakMilestoneJob>("Day streak achievements",
            x => x.Handle(CancellationToken.None), "5 15 * * *");
    }
}
