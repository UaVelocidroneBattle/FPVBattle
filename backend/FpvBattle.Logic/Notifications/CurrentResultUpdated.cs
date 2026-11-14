using FpvBattle.Domain;
using FpvBattle.Logic.Features.Cups;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record CurrentResultUpdated(
    Competition Competition,
    List<TrackTimeDelta> Deltas,
    CupOptions CupOptions
) : INotification;
