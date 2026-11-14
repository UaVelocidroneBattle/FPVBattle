using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamJoinRequestedNotification(Team Team, ApplicationUser Requester) : INotification;
