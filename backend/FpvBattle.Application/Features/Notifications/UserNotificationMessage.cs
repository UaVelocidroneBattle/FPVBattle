using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Notifications;

public record UserNotificationMessage(string UserId, NotificationType Type, string Text, string? CupId = null);
