using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamJoinRequestApprovedNotification(Team Team, ApplicationUser Member) : INotification;
