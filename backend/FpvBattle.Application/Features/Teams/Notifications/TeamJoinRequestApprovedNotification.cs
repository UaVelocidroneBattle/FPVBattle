using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Teams.Notifications;

public record TeamJoinRequestApprovedNotification(Team Team, ApplicationUser Member) : INotification;
