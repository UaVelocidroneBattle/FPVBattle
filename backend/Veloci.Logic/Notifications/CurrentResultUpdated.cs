using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CurrentResultUpdated(
    Competition Competition,
    List<TrackTimeDelta> Deltas,
    CupOptions CupOptions
) : INotification;
