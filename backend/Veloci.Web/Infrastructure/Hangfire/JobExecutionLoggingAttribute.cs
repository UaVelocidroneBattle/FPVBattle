using System.Diagnostics;
using Hangfire.Common;
using Hangfire.Server;
using Serilog;

namespace Veloci.Web.Infrastructure.Hangfire;

public class JobExecutionLoggingAttribute : JobFilterAttribute, IServerFilter
{
    private const string StopwatchKey = "JobExecutionStopwatch";

    public void OnPerforming(PerformingContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;
        var jobDescription = filterContext.BackgroundJob.Job.ToString(includeQueue: false);
        
        // Use actual job type for more readable logs
        var logger = Log.ForContext(filterContext.BackgroundJob.Job.Type);
        logger.Information("Job execution started: {JobId} - {JobDescription}", 
            jobId, jobDescription);
        
        // Store stopwatch for duration calculation - following Hangfire's internal pattern
        filterContext.Items[StopwatchKey] = Stopwatch.StartNew();
    }

    public void OnPerformed(PerformedContext filterContext)
    {
        var jobId = filterContext.BackgroundJob.Id;
        var jobDescription = filterContext.BackgroundJob.Job.ToString(includeQueue: false);
        
        // Calculate duration using stopwatch - idiomatic Hangfire pattern
        var duration = TimeSpan.Zero;
        if (filterContext.Items.TryGetValue(StopwatchKey, out var stopwatchObj) && 
            stopwatchObj is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            duration = stopwatch.Elapsed;
        }

        // Use actual job type for more readable logs
        var logger = Log.ForContext(filterContext.BackgroundJob.Job.Type);
        
        if (filterContext.Exception != null)
        {
            logger.Error(filterContext.Exception, 
                "Job execution failed: {JobId} - {JobDescription} after {Duration}ms", 
                jobId, jobDescription, duration.TotalMilliseconds);
        }
        else if (filterContext.Canceled)
        {
            logger.Warning("Job execution canceled: {JobId} - {JobDescription} after {Duration}ms", 
                jobId, jobDescription, duration.TotalMilliseconds);
        }
        else
        {
            logger.Information("Job execution completed: {JobId} - {JobDescription} in {Duration}ms", 
                jobId, jobDescription, duration.TotalMilliseconds);
        }
    }
}