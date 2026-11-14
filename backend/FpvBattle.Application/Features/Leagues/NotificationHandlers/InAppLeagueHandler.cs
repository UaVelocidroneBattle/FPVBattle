using FpvBattle.Application.Features.Leagues.Notifications;
using FpvBattle.Application.Features.Leagues.Services;
using FpvBattle.Application.Features.Notifications;
using FpvBattle.Application.Features.Notifications.Services;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Leagues.NotificationHandlers;

public class InAppLeagueHandler : INotificationHandler<LeagueUpdateNotification>
{
    private readonly UserNotificationService _notifications;
    private readonly InAppLeagueMessageComposer _composer;

    public InAppLeagueHandler(UserNotificationService notifications, InAppLeagueMessageComposer composer)
    {
        _notifications = notifications;
        _composer = composer;
    }

    public async Task Handle(LeagueUpdateNotification notification, CancellationToken cancellationToken)
    {
        var messages = notification.Updates
            .Select(update => new PilotNotificationMessage(
                update.Pilot,
                NotificationType.LeagueUpdate,
                _composer.LeagueUpdateMessage(update.OldLeague, update.NewLeague),
                notification.CupId))
            .ToList();

        await _notifications.NotifyPilotsAsync(messages, cancellationToken);
    }
}
