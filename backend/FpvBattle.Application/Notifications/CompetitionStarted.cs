using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

/// <param name="PickedByPatron">The pilot name of the patron who scheduled the track, or null.</param>
public record CompetitionStarted(
    Competition Competition,
    Track Track,
    CupOptions CupOptions,
    string? PickedByPatron
) : INotification;
