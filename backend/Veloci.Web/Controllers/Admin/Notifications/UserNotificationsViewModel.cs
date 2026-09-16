using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Admin.Notifications;

public class UserNotificationsViewModel
{
    public List<NotificationRow> Notifications { get; set; } = [];

    public string? Search { get; set; }

    /// <summary>Total matches in the database, which can exceed the rows shown.</summary>
    public int TotalCount { get; set; }

    public bool IsTruncated => TotalCount > Notifications.Count;
}

public class NotificationRow
{
    public DateTime CreatedOn { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string? PilotName { get; set; }
    public NotificationType Type { get; set; }
    public string? CupId { get; set; }
    public string? CupName { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime? ReadOn { get; set; }
}
