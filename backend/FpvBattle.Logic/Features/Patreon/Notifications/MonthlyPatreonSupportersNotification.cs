using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Features.Patreon.Notifications;

public record MonthlyPatreonSupportersNotification(List<PatreonSupporter> Supporters) : INotification;
