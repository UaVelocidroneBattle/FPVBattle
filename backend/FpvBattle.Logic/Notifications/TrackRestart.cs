using MediatR;

namespace FpvBattle.Logic.Notifications;

public record TrackRestart(string CupId) : INotification;
