using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Features.Teams.Notifications;

public record TeamJoinRequestedNotification(Team Team, ApplicationUser Requester) : INotification;
