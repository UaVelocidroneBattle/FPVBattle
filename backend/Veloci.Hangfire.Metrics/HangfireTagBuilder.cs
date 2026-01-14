using System;
using System.Diagnostics;
using Hangfire.Common;

namespace Veloci.Hangfire.Metrics
{
    /// <summary>
    /// Tag builder for creating standardized OpenTelemetry tag lists for Hangfire job metrics.
    /// </summary>
    public static class HangfireTagBuilder
    {
        // Tag name constants following OpenTelemetry semantic conventions
        private const string TagJobType = "job.type";
        private const string TagJobMethod = "job.method";
        private const string TagJobName = "job.name";
        private const string TagJobStatus = "job.status";
        private const string TagExceptionType = "exception.type";

        // Status values
        private const string StatusSuccess = "success";
        private const string StatusFailed = "failed";

        /// <summary>
        /// Creates a tag list with common job metadata (type, method, name).
        /// </summary>
        /// <param name="job">The Hangfire job.</param>
        /// <returns>Tag list with common job tags.</returns>
        public static TagList BuildCommonTags(Job? job)
        {
            var tags = new TagList();
            AddJobType(ref tags, job);
            AddJobMethod(ref tags, job);
            AddJobName(ref tags, job);
            return tags;
        }

        /// <summary>
        /// Creates a tag list with execution result tags (common tags + status + exception).
        /// </summary>
        /// <param name="job">The Hangfire job.</param>
        /// <param name="exception">The exception, if any occurred.</param>
        /// <returns>Tag list with execution result tags.</returns>
        public static TagList BuildExecutionTags(Job? job, Exception? exception)
        {
            var tags = new TagList();
            AddJobType(ref tags, job);
            AddJobMethod(ref tags, job);
            AddJobName(ref tags, job);
            AddStatus(ref tags, exception);
            AddException(ref tags, exception);
            return tags;
        }

        private static void AddJobType(ref TagList tags, Job? job)
        {
            var jobType = job?.Type?.FullName;
            if (!string.IsNullOrEmpty(jobType))
            {
                tags.Add(TagJobType, jobType);
            }
        }

        private static void AddJobMethod(ref TagList tags, Job? job)
        {
            var jobMethod = job?.Method?.Name;
            if (!string.IsNullOrEmpty(jobMethod))
            {
                tags.Add(TagJobMethod, jobMethod);
            }
        }

        private static void AddJobName(ref TagList tags, Job? job)
        {
            tags.Add(TagJobName, job?.ToString(false) ?? "unknown");
        }

        private static void AddStatus(ref TagList tags, Exception? exception)
        {
            tags.Add(TagJobStatus, exception is null ? StatusSuccess : StatusFailed);
        }

        private static void AddException(ref TagList tags, Exception? exception)
        {
            if (exception is not null)
            {
                tags.Add(TagExceptionType, exception.GetType().Name);
            }
        }
    }
}
