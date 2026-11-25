using MediatR;

namespace FpvBattle.Application.Notifications;

public record TrackRestart(string CupId, string? RestartedBy = null) : INotification;
