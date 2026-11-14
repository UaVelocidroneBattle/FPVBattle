using FpvBattle.Application.Features.Notifications;
using FpvBattle.Application.Features.Notifications.Services;
using FpvBattle.Application.Features.Teams.Notifications;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Teams.NotificationHandlers;

public class InAppTeamInviteHandler : INotificationHandler<TeamInviteSentNotification>
{
    private readonly UserNotificationService _notifications;

    public InAppTeamInviteHandler(UserNotificationService notifications)
    {
        _notifications = notifications;
    }

    public async Task Handle(TeamInviteSentNotification notification, CancellationToken cancellationToken)
    {
        var inviterPilotName = notification.Inviter.Pilot?.Name;

        var text = inviterPilotName is null
            ? $"You are invited to join the team {notification.Team.Name}"
            : $"{inviterPilotName} invited you to join the team {notification.Team.Name}";

        await _notifications.NotifyAsync(
            [new UserNotificationMessage(notification.Invitee.Id, NotificationType.TeamInvite, text)],
            cancellationToken);
    }
}
