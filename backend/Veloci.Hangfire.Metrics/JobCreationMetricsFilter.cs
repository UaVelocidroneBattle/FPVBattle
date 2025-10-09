using System;
using Hangfire.Client;

namespace Veloci.Hangfire.Metrics
{
    /// <summary>
    /// Client filter that tracks metrics for Hangfire job creation/enqueuing.
    /// </summary>
    public class JobCreationMetricsFilter : IClientFilter
    {
        /// <summary>
        /// Called before a job is created.
        /// </summary>
        /// <param name="context">The creating context.</param>
        public void OnCreating(CreatingContext context)
        {
        }

        /// <summary>
        /// Called after a job has been successfully created.
        /// </summary>
        /// <param name="context">The created context.</param>
        public void OnCreated(CreatedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var tagList = HangfireTagBuilder.BuildCommonTags(context.Job);
            HangfireMetrics.JobsCreated.Add(1, tagList);
        }
    }
}
