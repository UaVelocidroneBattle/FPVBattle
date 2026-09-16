using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Api.Notifications;

public class NotificationsModel
{
    public required IReadOnlyCollection<NotificationModel> Items { get; set; }

    public required int UnreadCount { get; set; }
}

public class NotificationModel
{
    public required int Id { get; set; }

    public required NotificationType Type { get; set; }

    public string? CupId { get; set; }

    public required string Text { get; set; }

    public required DateTime CreatedOn { get; set; }

    public DateTime? ReadOn { get; set; }
}

public class UnreadCountModel
{
    public required int UnreadCount { get; set; }
}

public class MarkReadRequest
{
    public required List<int> Ids { get; set; }
}
