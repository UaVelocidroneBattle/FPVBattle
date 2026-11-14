using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Features.Teams.Notifications;

public record TeamJoinRequestApprovedNotification(Team Team, ApplicationUser Member) : INotification;
