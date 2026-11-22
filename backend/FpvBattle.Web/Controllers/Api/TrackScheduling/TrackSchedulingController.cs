using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Web.Controllers.Api.TrackScheduling;

[ApiController]
[Route("/api/track-scheduling/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TrackSchedulingController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TrackSchedulingService _scheduling;
    private readonly TrackQueueService _queue;
    private readonly PatronQuotaPolicy _quotaPolicy;
    private readonly IApplicationDbContext _db;
    private readonly TrackSchedulingOptions _options;

    public TrackSchedulingController(
        UserManager<ApplicationUser> userManager,
        TrackSchedulingService scheduling,
        TrackQueueService queue,
        PatronQuotaPolicy quotaPolicy,
        IApplicationDbContext db,
        IOptions<TrackSchedulingOptions> options)
    {
        _userManager = userManager;
        _scheduling = scheduling;
        _queue = queue;
        _quotaPolicy = quotaPolicy;
        _db = db;
        _options = options.Value;
    }

    /// <summary>
    /// Whether the user can schedule a track, their monthly quota, the cups with their quads, and the user's waiting tracks.
    /// A visitor gets the reason to sign in.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<TrackSchedulingOverviewModel>> Overview()
    {
        var user = await _userManager.GetUserAsync(User);
        var quota = user is null ? null : await _quotaPolicy.GetQuotaAsync(user, DateTime.UtcNow);

        return new TrackSchedulingOverviewModel
        {
            Enabled = _options.Enabled,
            BlockedReason = quota is null ? TrackSchedulingMessages.SignIn : PatronQuotaPolicy.DescribeBlocker(quota),
            MonthlyLimit = quota?.MonthlyLimit ?? 0,
            UsedThisMonth = quota?.UsedThisMonth ?? 0,
            Remaining = quota?.Remaining ?? 0,
            Cups = (await _scheduling.GetCupsAsync()).Select(ToModel).ToList(),
            MyTracks = user is null ? [] : (await _scheduling.GetQueuePlacesAsync(user.Id)).Select(ToModel).ToList()
        };
    }

    [HttpGet]
    public async Task<ActionResult<List<SchedulableTrackModel>>> Suggestions([FromQuery] string cupId, [FromQuery] string? term)
    {
        if (!_options.Enabled)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var suggestions = await _scheduling.SuggestAsync(cupId, term ?? string.Empty, SchedulingRules.ForPatron(user));

        return suggestions.Select(suggestion => new SchedulableTrackModel
        {
            TrackId = suggestion.Track.Id,
            Name = suggestion.Track.Name,
            SceneName = suggestion.Track.Scene.Name,
            Restriction = suggestion.BlockingIssue?.Message
        }).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<QueuedTrackModel>> Schedule([FromBody] ScheduleTrackModel model)
    {
        if (!_options.Enabled)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var result = await _scheduling.ScheduleAsync(new ScheduleTrackRequest(model.CupId, model.TrackId, model.QuadId), SchedulingRules.ForPatron(user));

        return result.QueuedTrack is { } entry ? ToModel(await _queue.GetPlaceAsync(entry)) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult> Cancel([FromBody] QueuedTrackReferenceModel model)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        var entry = await _db.QueuedTracks.ScheduledBy(userId).FirstOrDefaultAsync(q => q.Id == model.Id);

        if (entry is null)
            return NotFound("Queued track not found");

        return await _scheduling.CancelAsync(entry) == CancelQueuedTrackResult.AlreadyRaced
            ? BadRequest("This track has already been raced")
            : NoContent();
    }

    private static SchedulingCupModel ToModel(SchedulingCup cup) => new()
    {
        CupId = cup.CupId,
        Name = cup.Name,
        Quads = cup.Quads.Select(q => new SchedulingQuadModel { Id = q.Id, Name = q.Name }).ToList()
    };

    private static QueuedTrackModel ToModel(QueuePlace place) => new()
    {
        Id = place.QueuedTrack.Id,
        CupId = place.QueuedTrack.CupId,
        TrackName = place.QueuedTrack.Track.Name,
        SceneName = place.QueuedTrack.Track.Scene.Name,
        QuadName = place.QueuedTrack.Quad?.Name,
        AddedOn = place.QueuedTrack.AddedOn,
        QueuePosition = place.Position
    };
}
