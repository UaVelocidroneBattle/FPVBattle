using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Notifications.Services;

namespace Veloci.Web.Controllers.Api.Notifications;

[ApiController]
[Route("/api/notifications")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NotificationsController : ControllerBase
{
    private const int DefaultTake = 20;
    private const int MaxTake = 100;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InAppNotificationService _notifications;

    public NotificationsController(UserManager<ApplicationUser> userManager, InAppNotificationService notifications)
    {
        _userManager = userManager;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationsModel>> Get([FromQuery] int take = DefaultTake, CancellationToken ct = default)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        var items = await _notifications.GetNewestAsync(userId, Math.Clamp(take, 1, MaxTake), ct);

        return new NotificationsModel
        {
            Items = items.Select(ToModel).ToList(),
            UnreadCount = await _notifications.GetUnreadCountAsync(userId, ct)
        };
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadCountModel>> UnreadCount(CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        return new UnreadCountModel { UnreadCount = await _notifications.GetUnreadCountAsync(userId, ct) };
    }

    [HttpPost("read")]
    public async Task<ActionResult<UnreadCountModel>> MarkRead([FromBody] MarkReadRequest request, CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        if (request.Ids.Count > MaxTake)
            return BadRequest($"No more than {MaxTake} notifications can be marked at once");

        await _notifications.MarkReadAsync(userId, request.Ids, ct);

        return new UnreadCountModel { UnreadCount = await _notifications.GetUnreadCountAsync(userId, ct) };
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<UnreadCountModel>> MarkAllRead(CancellationToken ct)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        await _notifications.MarkAllReadAsync(userId, ct);

        return new UnreadCountModel { UnreadCount = 0 };
    }

    private static NotificationModel ToModel(UserNotification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        CupId = notification.CupId,
        Text = notification.Text,
        CreatedOn = notification.CreatedOn,
        ReadOn = notification.ReadOn
    };
}
