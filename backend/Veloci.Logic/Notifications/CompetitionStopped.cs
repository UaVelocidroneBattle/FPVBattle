using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CompetitionStopped(
    Competition Competition,
    CupOptions CupOptions
) : INotification;
