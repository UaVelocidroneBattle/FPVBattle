using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record NewPatreonSupporterNotification(PatreonSupporter Supporter) : INotification;