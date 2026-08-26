using System.Text.Json.Serialization;

namespace Veloci.Data.Domain;

public class UserNotification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public NotificationType Type { get; set; }
    public string? CupId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? ReadOn { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<NotificationType>))]
public enum NotificationType
{
    General = 0,
    Achievement = 1,
    CompetitionResults = 2,
    LeagueUpdate = 3
}

public static class UserNotificationExtensions
{
    public static IQueryable<UserNotification> ForUser(this IQueryable<UserNotification> query, string userId)
    {
        return query.Where(n => n.UserId == userId);
    }

    public static IQueryable<UserNotification> Unread(this IQueryable<UserNotification> query)
    {
        return query.Where(n => n.ReadOn == null);
    }

    public static IQueryable<UserNotification> Newest(this IQueryable<UserNotification> query)
    {
        return query.OrderByDescending(n => n.CreatedOn);
    }
}
