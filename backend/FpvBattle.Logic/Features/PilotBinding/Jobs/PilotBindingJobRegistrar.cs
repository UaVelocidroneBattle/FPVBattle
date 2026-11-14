using FpvBattle.Logic.Jobs;
using Hangfire;

namespace FpvBattle.Logic.Features.PilotBinding.Jobs;

public class PilotBindingJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<ExpiredClaimCleanupJob>("Cleanup expired pilot claims", x => x.ExecuteAsync(CancellationToken.None), "30 4 * * *");
    }
}
