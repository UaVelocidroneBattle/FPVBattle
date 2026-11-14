using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Logic.Features.Patreon.Notifications;

public record NewPatreonSupporterNotification(PatreonSupporter Supporter) : INotification;
