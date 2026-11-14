using MediatR;
using Veloci.Domain;

namespace Veloci.Logic.Notifications;

public record CompetitionCancelled (Competition Competition) : INotification;
