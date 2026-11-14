using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Patreon.Notifications;

public record MonthlyPatreonSupportersNotification(List<PatreonSupporter> Supporters) : INotification;
