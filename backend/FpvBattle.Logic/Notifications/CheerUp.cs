using FpvBattle.Logic.Bot;
using MediatR;

namespace FpvBattle.Logic.Notifications;

public record CheerUp(string CupId, ChatMessageType Type) : INotification;
