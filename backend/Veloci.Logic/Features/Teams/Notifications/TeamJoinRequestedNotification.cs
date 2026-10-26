using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams.Notifications;

public record TeamJoinRequestedNotification(Team Team, ApplicationUser Requester) : INotification;
