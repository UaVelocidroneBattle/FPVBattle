namespace Veloci.Logic.Features.Notifications.Channels;

public interface INotificationChannel
{
    Task SendAsync(IReadOnlyCollection<UserNotificationMessage> messages, CancellationToken ct);
}
