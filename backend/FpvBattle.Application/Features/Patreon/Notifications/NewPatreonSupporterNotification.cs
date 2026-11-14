using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Features.Patreon.Notifications;

public record NewPatreonSupporterNotification(PatreonSupporter Supporter) : INotification;
