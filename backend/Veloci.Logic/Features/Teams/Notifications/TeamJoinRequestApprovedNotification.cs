using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamJoinRequestApprovedNotification(Team Team, ApplicationUser Member) : INotification;
