using FpvBattle.Logic.Features.Leagues.Services;
using FpvBattle.Logic.Jobs;
using Hangfire;

namespace FpvBattle.Logic.Features.Leagues.Jobs;

public class LeagueJobsRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<PaceRatingCalculator>("Calculate pace rating", x => x.CalculateAsync(), "0 3 * * 1");
        RecurringJob.AddOrUpdate<LeagueService>("Update pilots league", x => x.UpdatePilotLeaguesAsync(), "7 0 1 * *");
    }
}
