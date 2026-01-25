using MediatR;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Notifications;

public record CurrentResultUpdateMessage(
    Competition Competition,
    List<TrackTimeDelta> Deltas,
    CupOptions CupOptions
) : INotification;
