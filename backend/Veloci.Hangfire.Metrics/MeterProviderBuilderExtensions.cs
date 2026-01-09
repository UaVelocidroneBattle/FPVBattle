using System;
using System.Linq;
using Hangfire;
using OpenTelemetry.Metrics;

namespace Veloci.Hangfire.Metrics
{
    /// <summary>
    /// Extension methods to simplify registering Hangfire instrumentation.
    /// </summary>
    public static class HangfireMeterProviderBuilderExtensions
    {
        /// <summary>
        /// Enables Hangfire instrumentation to collect metrics about job execution performance.
        /// </summary>
        /// <param name="builder">The <see cref="MeterProviderBuilder"/> being configured.</param>
        /// <returns>The instance of <see cref="MeterProviderBuilder"/> to chain calls.</returns>
        /// <remarks>
        /// This instrumentation collects the following metrics:
        /// <list type="bullet">
        /// <item><description>hangfire.job.created - Counter of jobs created/enqueued</description></item>
        /// <item><description>hangfire.job.duration - Histogram of job execution duration</description></item>
        /// <item><description>hangfire.job.executions - Counter of total job executions</description></item>
        /// <item><description>hangfire.job.active - Gauge of currently executing jobs</description></item>
        /// <item><description>hangfire.job.queue.duration - Histogram of time jobs spend in queue</description></item>
        /// </list>
        /// </remarks>
        public static MeterProviderBuilder AddHangfireInstrumentation(
            this MeterProviderBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Register the Meter name that HangfireMetrics uses
            builder.AddMeter("Veloci.Hangfire");

            // Register the server filter that collects execution metrics
            if (!GlobalJobFilters.Filters.Any(f => f.Instance is MetricsFilter))
            {
                GlobalJobFilters.Filters.Add(new MetricsFilter());
            }

            // Register the client filter that collects job creation metrics
            if (!GlobalJobFilters.Filters.Any(f => f.Instance is JobCreationMetricsFilter))
            {
                GlobalJobFilters.Filters.Add(new JobCreationMetricsFilter());
            }

            return builder;
        }
    }
}
