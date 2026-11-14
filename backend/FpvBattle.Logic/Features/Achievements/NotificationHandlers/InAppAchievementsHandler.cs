using FpvBattle.Domain;
using FpvBattle.Logic.Features.Achievements.Notifications;
using FpvBattle.Logic.Features.Achievements.Services;
using FpvBattle.Logic.Features.Notifications;
using FpvBattle.Logic.Features.Notifications.Services;
using MediatR;

namespace FpvBattle.Logic.Features.Achievements.NotificationHandlers;

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
