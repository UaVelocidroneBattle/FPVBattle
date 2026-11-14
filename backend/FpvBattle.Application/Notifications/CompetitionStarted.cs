using FpvBattle.Application.Features.Cups;
using FpvBattle.Domain;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record CompetitionStarted(
    Competition Competition,
    Track Track,
    CupOptions CupOptions
) : INotification;
