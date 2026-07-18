using Hangfire;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.PilotBinding.Jobs;

public class PilotBindingJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<ExpiredClaimCleanupJob>("Cleanup expired pilot claims", x => x.ExecuteAsync(CancellationToken.None), "15 * * * *");
    }
}
