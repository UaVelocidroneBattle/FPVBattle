using System;
using System.Diagnostics;
using Hangfire.Server;

namespace Veloci.Hangfire.Metrics
{
    public class MetricsFilter : IServerFilter
    {
        private const string StopwatchContextKey = "metrics_start_time";
        private const string StartTimeContextKey = "metrics_performing_start";

        public void OnPerforming(PerformingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Items[StopwatchContextKey] = Stopwatch.StartNew();
            context.Items[StartTimeContextKey] = DateTime.UtcNow;

            var tagList = BuildPerformingTags(context);
            HangfireMetrics.ActiveJobs.Add(1, tagList);
        }

        public void OnPerformed(PerformedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Items.TryGetValue(StopwatchContextKey, out var stopwatchObj) || stopwatchObj is not Stopwatch sw)
            {
                return;
            }

            sw.Stop();

            var durationSeconds = sw.Elapsed.TotalSeconds;
            var tagList = BuildTags(context);

            // Record execution metrics
            HangfireMetrics.JobDurationHistogram.Record(durationSeconds, tagList);
            HangfireMetrics.JobExecutions.Add(1, tagList);
            HangfireMetrics.ActiveJobs.Add(-1, tagList);

            // Record queue latency (time waiting in queue before execution started)
            if (context.Items.TryGetValue(StartTimeContextKey, out var startTimeObj) && startTimeObj is DateTime startTime)
            {
                var createdAt = context.BackgroundJob.CreatedAt;
                var queueLatencySeconds = (startTime - createdAt).TotalSeconds;
                HangfireMetrics.JobQueueLatencyHistogram.Record(queueLatencySeconds, tagList);
            }
        }

        private static TagList BuildTags(PerformedContext context)
        {
            var job = context.BackgroundJob?.Job;
            return HangfireTagBuilder.BuildExecutionTags(job, context.Exception);
        }

        private static TagList BuildPerformingTags(PerformingContext context)
        {
            var job = context.BackgroundJob?.Job;
            return HangfireTagBuilder.BuildCommonTags(job);
        }
    }
}
