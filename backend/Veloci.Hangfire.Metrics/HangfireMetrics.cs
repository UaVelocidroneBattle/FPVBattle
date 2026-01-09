using System.Diagnostics.Metrics;

namespace Veloci.Hangfire.Metrics
{
    /// <summary>
    /// Centralized metrics definitions for Hangfire instrumentation.
    /// </summary>
    internal static class HangfireMetrics
    {
        /// <summary>
        /// The meter instance for all Hangfire metrics.
        /// </summary>
        public static readonly Meter Meter = new("Veloci.Hangfire", "1.0.0");

        /// <summary>
        /// Counter for total number of jobs created/enqueued.
        /// </summary>
        public static readonly Counter<long> JobsCreated =
            Meter.CreateCounter<long>("hangfire.job.created",
                description: "Total number of jobs created/enqueued.");

        /// <summary>
        /// Counter for total number of job executions.
        /// </summary>
        public static readonly Counter<long> JobExecutions =
            Meter.CreateCounter<long>("hangfire.job.executions",
                description: "Total number of job executions.");

        /// <summary>
        /// Histogram for job execution duration.
        /// </summary>
        public static readonly Histogram<double> JobDurationHistogram =
            Meter.CreateHistogram<double>("hangfire.job.duration", unit: "s",
                description: "Duration of Hangfire job execution.");

        /// <summary>
        /// Up/down counter for currently executing jobs.
        /// </summary>
        public static readonly UpDownCounter<int> ActiveJobs =
            Meter.CreateUpDownCounter<int>("hangfire.job.active",
                description: "Number of currently executing jobs.");

        /// <summary>
        /// Histogram for time jobs spend waiting in queue.
        /// </summary>
        public static readonly Histogram<double> JobQueueLatencyHistogram =
            Meter.CreateHistogram<double>("hangfire.job.queue.latency", unit: "s",
                description: "Time jobs spend waiting in queue before execution.");
    }
}
