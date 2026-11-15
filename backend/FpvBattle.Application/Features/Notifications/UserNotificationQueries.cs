using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Notifications;

public static class UserNotificationQueries
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
