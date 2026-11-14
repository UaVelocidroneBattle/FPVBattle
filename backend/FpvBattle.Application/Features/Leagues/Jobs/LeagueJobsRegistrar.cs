using FpvBattle.Application.Features.Leagues.Services;
using FpvBattle.Application.Jobs;
using Hangfire;

namespace FpvBattle.Application.Features.Leagues.Jobs;

public class LeagueJobsRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<PaceRatingCalculator>("Calculate pace rating", x => x.CalculateAsync(), "0 3 * * 1");
        RecurringJob.AddOrUpdate<LeagueService>("Update pilots league", x => x.UpdatePilotLeaguesAsync(), "7 0 1 * *");
    }
}
