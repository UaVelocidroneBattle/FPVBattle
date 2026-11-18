using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Patreon.Notifications;

public record NewPatreonSupportersNotification(IReadOnlyList<PatreonSupporter> Supporters) : INotification;
