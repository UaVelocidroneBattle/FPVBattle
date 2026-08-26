using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Notifications;

public record UserNotificationMessage(string UserId, NotificationType Type, string Text, string? CupId = null);
