using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record CompetitionStopped (Competition Competition) : INotification;
