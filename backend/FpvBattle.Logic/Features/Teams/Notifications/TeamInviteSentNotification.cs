using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Features.Teams.Notifications;

public record TeamInviteSentNotification(Team Team, ApplicationUser Inviter, ApplicationUser Invitee) : INotification;
