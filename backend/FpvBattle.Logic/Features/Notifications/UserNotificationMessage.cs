using FpvBattle.Domain;

namespace FpvBattle.Logic.Features.Notifications;

public record UserNotificationMessage(string UserId, NotificationType Type, string Text, string? CupId = null);
