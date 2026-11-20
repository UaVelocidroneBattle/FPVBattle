using FpvBattle.Application.Jobs;
using Hangfire;

namespace FpvBattle.Application.Features.Tracks.Jobs;

public class TracksJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<SyncTrackLookupsJob>(
            "Sync track lookups",
            x => x.Handle(CancellationToken.None),
            "5 23 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}
