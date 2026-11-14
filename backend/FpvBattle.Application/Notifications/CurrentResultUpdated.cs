using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record CurrentResultUpdated(
    Competition Competition,
    List<TrackTimeDelta> Deltas,
    CupOptions CupOptions
) : INotification;
