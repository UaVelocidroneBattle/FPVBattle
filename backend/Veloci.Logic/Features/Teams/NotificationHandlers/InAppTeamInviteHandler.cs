using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Notifications;
using Veloci.Logic.Features.Notifications.Services;
using Veloci.Logic.Features.Teams.Notifications;

namespace Veloci.Logic.Features.Teams.NotificationHandlers;

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
