using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Teams.Notifications;

public record TeamJoinRequestedNotification(Team Team, ApplicationUser Requester) : INotification;
