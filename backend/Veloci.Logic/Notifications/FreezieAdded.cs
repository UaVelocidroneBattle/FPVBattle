using MediatR;

namespace Veloci.Logic.Notifications;

public record FreezieAdded(string PilotName) : INotification;
