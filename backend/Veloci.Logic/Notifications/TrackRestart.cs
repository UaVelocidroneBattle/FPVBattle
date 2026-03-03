using MediatR;

namespace Veloci.Logic.Notifications;

public record TrackRestart(string CupId) : INotification;
