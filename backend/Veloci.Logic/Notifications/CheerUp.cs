using MediatR;
using Veloci.Logic.Bot;

namespace Veloci.Logic.Notifications;

public record CheerUp(string CupId, ChatMessage Message) : INotification;
