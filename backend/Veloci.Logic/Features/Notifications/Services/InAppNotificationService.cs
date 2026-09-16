using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Notifications.Services;

public class InAppNotificationService
{
    private readonly IRepository<UserNotification> _notifications;

    public InAppNotificationService(IRepository<UserNotification> notifications)
    {
        _notifications = notifications;
    }

    public async Task<IReadOnlyCollection<UserNotification>> GetNewestAsync(string userId, int take, CancellationToken ct)
    {
        return await _notifications
            .GetAll()
            .ForUser(userId)
            .Newest()
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct)
    {
        return await _notifications
            .GetAll()
            .ForUser(userId)
            .Unread()
            .CountAsync(ct);
    }

    public async Task MarkReadAsync(string userId, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _notifications
            .GetAll()
            .ForUser(userId)
            .Unread()
            .Where(n => ids.Contains(n.Id))
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.ReadOn, now), ct);
    }

    public async Task MarkAllReadAsync(string userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _notifications
            .GetAll()
            .ForUser(userId)
            .Unread()
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.ReadOn, now), ct);
    }
}
