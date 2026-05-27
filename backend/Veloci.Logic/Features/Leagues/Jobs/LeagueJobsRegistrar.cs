using Hangfire;
using Veloci.Logic.Features.Leagues.Services;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Leagues.Jobs;

public class LeagueJobsRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<PaceRatingCalculator>("Calculate pace rating", x => x.CalculateAsync(), "0 3 * * 1");
        RecurringJob.AddOrUpdate<LeagueService>("Update pilots league", x => x.UpdatePilotLeaguesAsync(), "7 0 1 * *");
    }
}
