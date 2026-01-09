using Hangfire;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Patreon.Jobs;

public class PatreonJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<PatreonSyncJob>("Patreon sync", x => x.Handle(CancellationToken.None), "0 9 * * *");
        RecurringJob.AddOrUpdate<MonthlyPatreonSupportersJob>("Patreon supporters", x => x.Handle(CancellationToken.None), "5 10 * * 1");
        RecurringJob.AddOrUpdate<AccrueFreeziesToPatronsJob>("Accrue freezies to patrons", x => x.Handle(CancellationToken.None), "8 10 1 * *");
    }
}
