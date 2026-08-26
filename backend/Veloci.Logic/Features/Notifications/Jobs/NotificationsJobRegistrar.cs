using Hangfire;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Notifications.Jobs;

public class NotificationsJobRegistrar : IJobRegistrar
{
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<NotificationCleanupJob>("Cleanup old notifications", x => x.ExecuteAsync(CancellationToken.None), "1 5 * * *");
    }
}
