using FpvBattle.Application.Persistence;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Notifications.Channels;

public class InAppNotificationChannel : INotificationChannel
{
    private readonly IApplicationDbContext _db;

    public InAppNotificationChannel(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SendAsync(IReadOnlyCollection<UserNotificationMessage> messages, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var notifications = messages.Select(message => new UserNotification
        {
            UserId = message.UserId,
            Type = message.Type,
            CupId = message.CupId,
            Text = message.Text,
            CreatedOn = now
        });

        _db.UserNotifications.AddRange(notifications);
        await _db.SaveChangesAsync(ct);
    }
}
