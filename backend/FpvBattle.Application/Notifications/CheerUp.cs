using FpvBattle.Application.Bot;
using MediatR;

namespace FpvBattle.Application.Notifications;

public record CheerUp(string CupId, ChatMessageType Type) : INotification;
