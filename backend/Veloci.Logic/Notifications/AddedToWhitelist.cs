using MediatR;

namespace Veloci.Logic.Notifications;

public record AddedToWhitelist(string PilotName) : INotification;
