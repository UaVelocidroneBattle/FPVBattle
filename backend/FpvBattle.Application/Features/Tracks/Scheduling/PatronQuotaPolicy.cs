using FpvBattle.Application.Features.Patreon;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <summary>
/// How many tracks a patron may still schedule this month: paying patrons only, up to their tier's monthly limit.
/// The month is the UTC calendar month the track was added in, and a cancelled track frees its slot.
/// </summary>
public class PatronQuotaPolicy
{
    private readonly IApplicationDbContext _db;
    private readonly TrackSchedulingOptions _options;

    public PatronQuotaPolicy(IApplicationDbContext db, IOptions<TrackSchedulingOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<PatronQuota> GetQuotaAsync(ApplicationUser user, DateTime utcNow)
    {
        var monthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = monthStart.AddMonths(1);

        if (user.PilotId is not { } pilotId)
            return new PatronQuota(PatronQuotaBlocker.PilotNotLinked, 0, 0, nextMonthStart);

        var tierNames = await _db.PatreonSupporters
            .Paying()
            .ForPilot(pilotId)
            .Select(s => s.TierName)
            .ToListAsync();

        if (tierNames.Count == 0)
            return new PatronQuota(PatronQuotaBlocker.NotPatron, 0, 0, nextMonthStart);

        var limit = tierNames.Max(_options.MonthlyLimitFor);
        var used = await _db.QueuedTracks.ScheduledBy(user.Id).AddedSince(monthStart).CountAsync();

        var blocker = limit == 0 ? PatronQuotaBlocker.TierNotIncluded
            : used >= limit ? PatronQuotaBlocker.MonthlyLimitReached
            : (PatronQuotaBlocker?)null;

        return new PatronQuota(blocker, limit, used, nextMonthStart);
    }

    public static string? DescribeBlocker(PatronQuota quota) => quota.Blocker switch
    {
        PatronQuotaBlocker.PilotNotLinked => TrackSchedulingMessages.PilotNotLinked,
        PatronQuotaBlocker.NotPatron => TrackSchedulingMessages.PatronsOnly,
        PatronQuotaBlocker.TierNotIncluded => TrackSchedulingMessages.TierNotIncluded,
        PatronQuotaBlocker.MonthlyLimitReached => TrackSchedulingMessages.MonthlyLimitReached(quota.MonthlyLimit, quota.NextMonthStart),
        _ => null
    };
}
