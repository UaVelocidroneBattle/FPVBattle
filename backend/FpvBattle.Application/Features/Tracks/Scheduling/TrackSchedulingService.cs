using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Selection;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Tracks.Scheduling;

/// <summary>
/// Adding tracks to a cup's queue, the same way for admins and patrons. Every track gets the same checks,
/// and the scheduler's <see cref="SchedulingRules"/> decide which of them block.
/// </summary>
public class TrackSchedulingService
{
    private readonly IApplicationDbContext _db;
    private readonly ICupService _cupService;
    private readonly TrackQueueService _queue;
    private readonly PatronQuotaPolicy _quotaPolicy;

    public TrackSchedulingService(
        IApplicationDbContext db,
        ICupService cupService,
        TrackQueueService queue,
        PatronQuotaPolicy quotaPolicy)
    {
        _db = db;
        _cupService = cupService;
        _queue = queue;
        _quotaPolicy = quotaPolicy;
    }

    /// <summary>The enabled cups, each with its Quad of the Day pool in configuration order.</summary>
    public async Task<List<SchedulingCup>> GetCupsAsync()
    {
        var cups = new List<SchedulingCup>();

        foreach (var cupId in _cupService.GetEnabledCupIds())
            cups.Add(new SchedulingCup(cupId, _cupService.GetCupOptions(cupId).Name, await GetQuadsAsync(cupId)));

        return cups;
    }

    public async Task<List<SchedulableTrack>> SuggestAsync(string cupId, string term, SchedulingRules rules)
    {
        if (!TrackSearch.IsSearchable(term) || !IsSchedulableCup(cupId))
            return [];

        var cupFilter = CupFilter(cupId);
        var tracks = SchedulableTracks();

        if (rules.HidesTracksOutsideCupFilter)
            tracks = cupFilter.WhereSceneAndTypeAllowed(tracks);

        var matches = await tracks.SearchByName(term).ToListAsync();

        // The name patterns can only run in memory, so a hidden track found this way leaves fewer than the maximum
        if (rules.HidesTracksOutsideCupFilter)
            matches = matches.Where(cupFilter.IsTrackSuitable).ToList();

        return await DescribeAsync(cupId, matches, rules);
    }

    public async Task<ScheduleTrackResult> ScheduleAsync(ScheduleTrackRequest request, SchedulingRules rules)
    {
        if (!IsSchedulableCup(request.CupId))
            return ScheduleTrackResult.Failed(TrackSchedulingMessages.CupNotAvailable);

        if (rules.Patron is { } patron
            && PatronQuotaPolicy.DescribeBlocker(await _quotaPolicy.GetQuotaAsync(patron, DateTime.UtcNow)) is { } quotaBlocker)
            return ScheduleTrackResult.Failed(quotaBlocker);

        var track = await SchedulableTracks().FirstOrDefaultAsync(t => t.Id == request.TrackId);

        if (track is null)
            return ScheduleTrackResult.Failed(TrackSchedulingMessages.TrackNotFound);

        if ((await DescribeAsync(request.CupId, [track], rules)).Single().BlockingIssue is { } blockingIssue)
            return ScheduleTrackResult.Failed(blockingIssue.Message);

        if (request.QuadId is { } quadId && (await GetQuadsAsync(request.CupId)).All(q => q.Id != quadId))
            return ScheduleTrackResult.Failed(TrackSchedulingMessages.QuadNotAllowed);

        if (request.ScheduledOn is { } date && await _queue.IsDateTakenAsync(request.CupId, date))
            return ScheduleTrackResult.Failed(TrackSchedulingMessages.DateTaken(date));

        var entry = new QueuedTrack
        {
            CupId = request.CupId,
            TrackId = track.Id,
            QuadId = request.QuadId,
            ScheduledOn = request.ScheduledOn,
            ScheduledByUserId = rules.Patron?.Id
        };

        try
        {
            await _queue.AddAsync(entry);
        }
        catch (TrackAlreadyQueuedException)
        {
            return ScheduleTrackResult.Failed(TrackSchedulingMessages.AlreadyScheduled);
        }

        return new ScheduleTrackResult(entry, null);
    }

    public async Task<CancelQueuedTrackResult> CancelAsync(QueuedTrack entry)
    {
        if (entry.Used)
            return CancelQueuedTrackResult.AlreadyRaced;

        await _queue.RemoveAsync(entry);

        return CancelQueuedTrackResult.Cancelled;
    }

    /// <summary>The patron's tracks still waiting in a queue, oldest first, with their places.</summary>
    public async Task<List<QueuePlace>> GetQueuePlacesAsync(string userId)
    {
        var entries = await _db.QueuedTracks
            .ScheduledBy(userId)
            .NotUsed()
            .Include(q => q.Track).ThenInclude(t => t.Scene)
            .Include(q => q.Quad)
            .OrderBy(q => q.AddedOn)
            .ToListAsync();

        var places = new List<QueuePlace>(entries.Count);

        foreach (var entry in entries)
            places.Add(await _queue.GetPlaceAsync(entry));

        return places;
    }

    private async Task<List<QuadModel>> GetQuadsAsync(string cupId)
    {
        var options = _cupService.GetCupOptions(cupId).QuadOfTheDay;

        if (!options.Enabled || options.Quads.Length == 0)
            return [];

        var quads = await _db.QuadModels.Where(q => options.Quads.Contains(q.Name)).ToListAsync();

        return quads.OrderBy(q => Array.IndexOf(options.Quads, q.Name)).ToList();
    }

    private async Task<List<SchedulableTrack>> DescribeAsync(string cupId, IReadOnlyList<Track> tracks, SchedulingRules rules)
    {
        var cupFilter = CupFilter(cupId);
        var trackIds = tracks.Select(t => t.Id).ToList();

        var queuedTrackIds = (await _db.QueuedTracks
            .NotUsed()
            .Where(q => trackIds.Contains(q.TrackId))
            .Select(q => q.TrackId)
            .ToListAsync()).ToHashSet();

        var lastRaces = await _db.Competitions.GetLastRacesAsync(trackIds);

        return tracks
            .Select(track =>
            {
                var lastRace = lastRaces.GetValueOrDefault(track.Id);
                var issues = FindIssues(track, cupFilter, queuedTrackIds.Contains(track.Id), lastRace)
                    .Select(issue => new TrackIssue(issue.Kind, issue.Message, rules.Blocks(issue.Kind)))
                    .ToList();

                return new SchedulableTrack(track, lastRace, issues);
            })
            .ToList();
    }

    private static IEnumerable<(TrackIssueKind Kind, string Message)> FindIssues(
        Track track, TrackFilter cupFilter, bool isQueued, LastRace? lastRace)
    {
        if (!cupFilter.IsTrackSuitable(track))
            yield return (TrackIssueKind.OutsideCupFilter, TrackSchedulingMessages.OutsideCupFilter);

        if (isQueued)
            yield return (TrackIssueKind.AlreadyQueued, TrackSchedulingMessages.AlreadyScheduled);

        if (!TrackReuseRules.IsWellRated(track.Rating))
            yield return (TrackIssueKind.BadlyRated, TrackSchedulingMessages.BadlyRated(track.Rating!.Value!.Value));

        if (lastRace is { IsInReuseCooldown: true })
            yield return (TrackIssueKind.RacedRecently, TrackSchedulingMessages.RacedRecently(lastRace.StartedOn));
    }

    private bool IsSchedulableCup(string cupId) => _cupService.GetEnabledCupIds().Contains(cupId);

    private IQueryable<Track> SchedulableTracks() => _db.Tracks.Available().Include(t => t.Scene).Include(t => t.Rating);

    private TrackFilter CupFilter(string cupId) => new(_cupService.GetCupOptions(cupId).TrackFilter);
}
