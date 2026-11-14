using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamInviteSentNotification(Team Team, ApplicationUser Inviter, ApplicationUser Invitee) : INotification;
