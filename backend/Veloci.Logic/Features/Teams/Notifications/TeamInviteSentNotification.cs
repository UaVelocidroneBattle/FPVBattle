using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamInviteSentNotification(Team Team, ApplicationUser Inviter, ApplicationUser Invitee) : INotification;
