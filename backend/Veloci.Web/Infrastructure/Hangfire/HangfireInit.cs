using Hangfire;
using Hangfire.Storage;
using Serilog;
using Veloci.Logic.Services;
using Veloci.Logic.Services.YearResults;

namespace Veloci.Web.Infrastructure.Hangfire;

public class HangfireInit
{
    public static void InitRecurrentJobs(IConfiguration configuration)
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
        RecurringJob.AddOrUpdate<CompetitionService>("Day streak potential lose", x => x.DayStreakPotentialLoseNotification(), "5 14 * * *");
        RecurringJob.AddOrUpdate<CompetitionConductor>("Vote reminder", x => x.VoteReminder(), "30 14 * * *");
        RecurringJob.AddOrUpdate<CompetitionConductor>("Stop poll", x => x.StopPollAsync(), "58 14 * * *");
        RecurringJob.AddOrUpdate<CompetitionConductor>("Stop competition", x => x.StopAsync(), "1 15 * * *");
        RecurringJob.AddOrUpdate<CompetitionConductor>("Season results", x => x.SeasonResultsAsync(), "2 15 * * *");
        RecurringJob.AddOrUpdate<CompetitionConductor>("Start new competition", x => x.StartNewAsync(), "3 15 * * *");
        RecurringJob.AddOrUpdate<CompetitionService>("Day streak achievements", x => x.PublishDayStreakAchievements(), "5 15 * * *");

        Log.Information("Setting up continuous monitoring recurring jobs");
        RecurringJob.AddOrUpdate<CompetitionService>("Update results", x => x.UpdateResultsAsync(), "*/10 * * * *");
        RecurringJob.AddOrUpdate<CompetitionService>("Publish current leaderboard", x => x.PublishCurrentLeaderboardAsync(), "1 */2 * * *");

        Log.Information("Setting up yearly recurring jobs");
        RecurringJob.AddOrUpdate<YearResultsService>("Year results", x => x.Publish(), "15 11 2 1 *");
        
        Log.Information("Hangfire recurring job initialization completed");
    }
}