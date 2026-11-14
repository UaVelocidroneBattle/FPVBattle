using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Logic.Features.Notifications.Jobs;

public class NotificationCleanupJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<NotificationCleanupJob>();

    public static readonly TimeSpan ReadRetention = TimeSpan.FromDays(30);
    public static readonly TimeSpan UnreadRetention = TimeSpan.FromDays(90);

    private readonly IApplicationDbContext _db;

    public NotificationCleanupJob(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var readBefore = now - ReadRetention;
        var createdBefore = now - UnreadRetention;

        var deleted = await _db.UserNotifications.Where(n => n.ReadOn <= readBefore || n.CreatedOn <= createdBefore)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            Log.Information("Deleted {Count} old notifications", deleted);
    }
}
