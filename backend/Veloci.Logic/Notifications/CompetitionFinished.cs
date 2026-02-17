using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CompetitionFinished(
    Competition Competition,
    CupOptions CupOptions
) : INotification;
