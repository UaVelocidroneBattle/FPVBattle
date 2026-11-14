using MediatR;

namespace FpvBattle.Application.Notifications;

public record TrackRestart(string CupId) : INotification;
