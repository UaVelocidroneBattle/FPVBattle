using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Web.Controllers.Admin.Notifications;

public class UserNotificationsController : AdminControllerBase
{
    private const int MaxRows = 500;

    private readonly IRepository<UserNotification> _notifications;
    private readonly IRepository<ApplicationUser> _users;
    private readonly ICupService _cupService;

    public UserNotificationsController(
        IRepository<UserNotification> notifications,
        IRepository<ApplicationUser> users,
        ICupService cupService)
    {
        _notifications = notifications;
        _users = users;
        _cupService = cupService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query =
            from notification in _notifications.GetAll()
            join user in _users.GetAll() on notification.UserId equals user.Id
            select new { Notification = notification, User = user };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";

            query = query.Where(x =>
                EF.Functions.Like(x.User.Email!, pattern) ||
                EF.Functions.Like(x.User.Pilot!.Name, pattern) ||
                EF.Functions.Like(x.Notification.Text, pattern));
        }

        var totalCount = await query.CountAsync();

        var rows = await query
            // A batch shares one CreatedOn, so the id keeps the order stable within it.
            .OrderByDescending(x => x.Notification.CreatedOn)
            .ThenByDescending(x => x.Notification.Id)
            .Take(MaxRows)
            .Select(x => new NotificationRow
            {
                CreatedOn = x.Notification.CreatedOn,
                UserEmail = x.User.Email ?? string.Empty,
                PilotName = x.User.Pilot != null ? x.User.Pilot.Name : null,
                Type = x.Notification.Type,
                CupId = x.Notification.CupId,
                Text = x.Notification.Text,
                ReadOn = x.Notification.ReadOn
            })
            .ToListAsync();

        foreach (var row in rows)
        {
            row.CupName = CupName(row.CupId);
        }

        return View(new UserNotificationsViewModel
        {
            Notifications = rows,
            Search = search,
            TotalCount = totalCount
        });
    }

    private string? CupName(string? cupId)
    {
        if (cupId is null)
            return null;

        // Falls back to the raw id so a cup removed from configuration still reads sensibly.
        return _cupService.CupExists(cupId) ? _cupService.GetCupOptions(cupId).Name : cupId;
    }
}
