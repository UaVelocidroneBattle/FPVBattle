using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Patreon.Notifications;

public record NewPatreonSupporterNotification(PatreonSupporter Supporter) : INotification;