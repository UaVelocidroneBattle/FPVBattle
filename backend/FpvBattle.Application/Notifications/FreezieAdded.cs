using MediatR;

namespace FpvBattle.Application.Notifications;

public record FreezieAdded(string PilotName) : INotification;
