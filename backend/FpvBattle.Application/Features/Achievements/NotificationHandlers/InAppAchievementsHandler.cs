using FpvBattle.Application.Features.Achievements.Notifications;
using FpvBattle.Application.Features.Achievements.Services;
using FpvBattle.Application.Features.Notifications;
using FpvBattle.Application.Features.Notifications.Services;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Achievements.NotificationHandlers;

public class InAppAchievementsHandler : INotificationHandler<GotAchievements>
{
    private readonly InAppAchievementMessageComposer _messageComposer;
    private readonly UserNotificationService _notifications;

    public InAppAchievementsHandler(
        InAppAchievementMessageComposer messageComposer,
        UserNotificationService notifications)
    {
        _messageComposer = messageComposer;
        _notifications = notifications;
    }

    public async Task Handle(GotAchievements notification, CancellationToken cancellationToken)
    {
        var messages = notification.Results
            .Select(result => new PilotNotificationMessage(
                result.Pilot,
                NotificationType.Achievement,
                _messageComposer.Achievement(result.Achievement),
                result.CupId))
            .ToList();

        await _notifications.NotifyPilotsAsync(messages, cancellationToken);
    }
}
