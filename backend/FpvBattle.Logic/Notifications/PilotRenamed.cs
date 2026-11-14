using MediatR;

namespace FpvBattle.Logic.Notifications;

public record PilotRenamed (string OldName, string NewName) : INotification;
