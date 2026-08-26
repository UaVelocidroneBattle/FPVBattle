using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Notifications.Jobs;

public class NotificationCleanupJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<NotificationCleanupJob>();

    public static readonly TimeSpan ReadRetention = TimeSpan.FromDays(30);
    public static readonly TimeSpan UnreadRetention = TimeSpan.FromDays(90);

    private readonly IRepository<UserNotification> _notifications;

    public NotificationCleanupJob(IRepository<UserNotification> notifications)
    {
        _notifications = notifications;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var readBefore = now - ReadRetention;
        var createdBefore = now - UnreadRetention;

        var deleted = await _notifications
            .GetAll(n => n.ReadOn <= readBefore || n.CreatedOn <= createdBefore)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            Log.Information("Deleted {Count} old notifications", deleted);
    }
}
