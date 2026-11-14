using FpvBattle.Domain;
using FpvBattle.Logic.Features.Cups;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record CompetitionStarted(
    Competition Competition,
    Track Track,
    CupOptions CupOptions
) : INotification;
