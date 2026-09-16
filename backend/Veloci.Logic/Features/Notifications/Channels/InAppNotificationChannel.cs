using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Notifications.Channels;

public class InAppNotificationChannel : INotificationChannel
{
    private readonly IRepository<UserNotification> _notifications;

    public InAppNotificationChannel(IRepository<UserNotification> notifications)
    {
        _notifications = notifications;
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

        await _notifications.AddRangeAsync(notifications);
        await _notifications.SaveChangesAsync(ct);
    }
}
