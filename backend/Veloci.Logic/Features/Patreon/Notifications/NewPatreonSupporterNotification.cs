using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Features.Patreon.Notifications;

public record NewPatreonSupporterNotification(PatreonSupporter Supporter) : INotification;
