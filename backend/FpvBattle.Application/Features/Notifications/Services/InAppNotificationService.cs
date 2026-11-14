using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Notifications.Services;

public class InAppNotificationService
{
    private readonly IApplicationDbContext _db;

    public InAppNotificationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<UserNotification>> GetNewestAsync(string userId, int take, CancellationToken ct)
    {
        return await _db.UserNotifications
            .ForUser(userId)
            .Newest()
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct)
    {
        return await _db.UserNotifications
            .ForUser(userId)
            .Unread()
            .CountAsync(ct);
    }

    public async Task MarkReadAsync(string userId, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _db.UserNotifications
            .ForUser(userId)
            .Unread()
            .Where(n => ids.Contains(n.Id))
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.ReadOn, now), ct);
    }

    public async Task MarkAllReadAsync(string userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await _db.UserNotifications
            .ForUser(userId)
            .Unread()
            .ExecuteUpdateAsync(n => n.SetProperty(x => x.ReadOn, now), ct);
    }
}
