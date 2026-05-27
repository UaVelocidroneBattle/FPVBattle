using Hangfire;
using Hangfire.Storage;
using Serilog;
using Veloci.Logic.Jobs;
using Veloci.Logic.Services;

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

        RecurringJob.AddOrUpdate<PilotService>("Day streak lose warning", x => x.DayStreakPotentialLoseNotificationAsync(), "03 22 * * *");
        RecurringJob.AddOrUpdate<PilotService>("Update pilots day streaks", x => x.UpdatePilotDayStreaksAsync(), "5 0 * * *");

        // Need to rework
        // RecurringJob.AddOrUpdate<StatisticsService>("End of season statistics", x => x.PublishEndOfSeasonStatisticsAsync(), "15 15 1 * *");

        Log.Information("Setting up continuous monitoring recurring jobs");

        var resultsUpdateEnabled = configuration.GetValue<bool>("ResultsUpdateEnabled");
        var resultsUpdateSchedule = resultsUpdateEnabled ? "*/10 * * * *" : Cron.Never();
        RecurringJob.AddOrUpdate<CompetitionService>("Update results", x => x.UpdateResultsAsync(), resultsUpdateSchedule);

        RecurringJob.AddOrUpdate<CompetitionService>("Publish current leaderboard", x => x.PublishCurrentLeaderboardAsync(), "1 */2 * * *");

        // Need to rework
        // RecurringJob.AddOrUpdate<YearResultsService>("Year results", x => x.Publish(), "15 11 2 1 *");

        RecurringJob.AddOrUpdate<ModelsService>("Update models", x => x.UpdateModelsAsync(), "33 3 * * *");

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
