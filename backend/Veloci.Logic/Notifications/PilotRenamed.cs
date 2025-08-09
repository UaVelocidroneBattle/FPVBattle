using MediatR;

namespace Veloci.Logic.Notifications;

public record PilotRenamed (string OldName, string NewName) : INotification;
