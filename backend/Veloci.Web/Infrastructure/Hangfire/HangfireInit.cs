using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Veloci.Logic.Jobs;
using Veloci.Logic.Services;
using Veloci.Logic.Services.Statistics;
using Veloci.Logic.Services.Statistics.YearResults;

namespace Veloci.Web.Infrastructure.Hangfire;

public class HangfireInit
{
    public static void InitRecurrentJobs(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        Log.Information("Initializing Hangfire recurring jobs");

        using (var connection = JobStorage.Current.GetConnection())
        {
            var existingJobs = connection.GetRecurringJobs();
            Log.Information("Removing {JobCount} existing recurring jobs", existingJobs.Count);

            foreach (var recurringJob in existingJobs)
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
            }
        }

        Log.Information("Setting up daily competition schedule recurring jobs");

        RecurringJob.AddOrUpdate<PilotService>("Day streak lose warning", x => x.DayStreakPotentialLoseNotificationAsync(), "30 22 * * *");
        RecurringJob.AddOrUpdate<PilotService>("Update pilots day streaks", x => x.UpdatePilotDayStreaksAsync(), "5 0 * * *");

        RecurringJob.AddOrUpdate<CompetitionConductor>("Season results", x => x.SeasonResultsAsync(), "2 15 * * *");
        RecurringJob.AddOrUpdate<StatisticsService>("End of season statistics", x => x.PublishEndOfSeasonStatisticsAsync(), "15 15 1 * *");

        Log.Information("Setting up continuous monitoring recurring jobs");

        RecurringJob.AddOrUpdate<CompetitionService>("Update results", x => x.UpdateResultsAsync(), "*/10 * * * *");
        RecurringJob.AddOrUpdate<CompetitionService>("Publish current leaderboard", x => x.PublishCurrentLeaderboardAsync(), "1 */2 * * *");

        Log.Information("Setting up yearly recurring jobs");

        RecurringJob.AddOrUpdate<YearResultsService>("Year results", x => x.Publish(), "15 11 2 1 *");

        Log.Information("Registering feature-specific jobs");
        using (var scope = serviceProvider.CreateScope())
        {
            var jobRegistrars = scope.ServiceProvider.GetServices<IJobRegistrar>();
            foreach (var registrar in jobRegistrars)
            {
                registrar.RegisterJobs();
            }
        }

        Log.Information("Hangfire recurring job initialization completed");
    }
}
