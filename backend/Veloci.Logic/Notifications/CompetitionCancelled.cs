using MediatR;
using Veloci.Data.Domain;

namespace Veloci.Logic.Notifications;

public record CompetitionCancelled (Competition Competition) : INotification;
