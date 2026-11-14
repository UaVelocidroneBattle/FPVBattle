using MediatR;

namespace FpvBattle.Application.Notifications;

public record PilotRenamed (string OldName, string NewName) : INotification;
