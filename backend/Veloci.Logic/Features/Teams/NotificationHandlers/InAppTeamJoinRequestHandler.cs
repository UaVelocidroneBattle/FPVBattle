using MediatR;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Notifications;
using Veloci.Logic.Features.Notifications.Services;
using Veloci.Logic.Features.Teams.Notifications;

namespace Veloci.Logic.Features.Teams.NotificationHandlers;

public class InAppTeamJoinRequestHandler :
    INotificationHandler<TeamJoinRequestedNotification>,
    INotificationHandler<TeamJoinRequestApprovedNotification>
{
    private readonly UserNotificationService _notifications;
    private readonly IRepository<TeamMember> _members;

    public InAppTeamJoinRequestHandler(UserNotificationService notifications, IRepository<TeamMember> members)
    {
        _notifications = notifications;
        _members = members;
    }

    /// <summary>The owner and admins are the ones who can approve the request.</summary>
    public async Task Handle(TeamJoinRequestedNotification notification, CancellationToken cancellationToken)
    {
        var approverIds = await _members.GetAll()
            .Where(m => m.TeamId == notification.Team.Id && (m.Role == TeamRole.Owner || m.Role == TeamRole.Admin))
            .Select(m => m.UserId)
            .ToListAsync(cancellationToken);

        var pilotName = notification.Requester.Pilot?.Name ?? "A pilot";
        var text = $"{pilotName} asked to join your team {notification.Team.Name}";

        var messages = approverIds
            .Select(userId => new UserNotificationMessage(userId, NotificationType.TeamJoinRequest, text))
            .ToList();

        await _notifications.NotifyAsync(messages, cancellationToken);
    }

    public async Task Handle(TeamJoinRequestApprovedNotification notification, CancellationToken cancellationToken)
    {
        var text = $"Your request to join the team {notification.Team.Name} was approved";

        await _notifications.NotifyAsync(
            [new UserNotificationMessage(notification.Member.Id, NotificationType.TeamJoinRequestApproved, text)],
            cancellationToken);
    }
}
