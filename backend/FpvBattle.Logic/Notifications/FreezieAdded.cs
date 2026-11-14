using MediatR;

namespace FpvBattle.Logic.Notifications;

public record FreezieAdded(string PilotName) : INotification;
