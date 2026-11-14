namespace FpvBattle.Application.Features.Notifications.Channels;

public interface INotificationChannel
{
    Task SendAsync(IReadOnlyCollection<UserNotificationMessage> messages, CancellationToken ct);
}
