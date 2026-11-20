using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Tracks;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Services.Tracks;

public class TrackQueueService
{
    public const int MinSuggestionTermLength = 2;
    private const int MaxSuggestions = 40;

    private static readonly ILogger Log = Serilog.Log.ForContext<TrackQueueService>();
    private readonly ICupService _cupService;
    private readonly IApplicationDbContext _db;

    public TrackQueueService(
        ICupService cupService,
        IApplicationDbContext db)
    {
        _cupService = cupService;
        _db = db;
    }

    public async Task QueueTrackAsync(string cupId, string trackId, DateTime? scheduleOn, int? quadId)
    {
        Log.Information("Queuing track {TrackId} for cup {CupId}, scheduled on {ScheduledOn}",
            trackId, cupId, (object?)scheduleOn ?? "next available date");

        if (scheduleOn.HasValue && AlreadyScheduledOnTheDate(cupId, scheduleOn.Value))
        {
            throw new Exception($"There is already a track scheduled on the date {scheduleOn.Value}");
        }

        var track = await QueueableTracks().FirstOrDefaultAsync(t => t.Id == trackId)
                    ?? throw new Exception("The track was not found or is no longer available");

        if (!CupFilter(cupId).IsTrackSuitable(track))
            Log.Information("Track '{TrackName}' is outside the track filter of cup {CupId}, queuing it anyway", track.Name, cupId);

        QuadModel? quad = null;

        if (quadId is not null)
        {
            quad = await _db.QuadModels.FindAsync(quadId.Value);
        }

        var queuedTrack = new QueuedTrack
        {
            TrackId = track.Id,
            ScheduledOn = scheduleOn,
            AddedOn = DateTime.UtcNow,
            CupId = cupId,
            Quad = quad
        };

        _db.QueuedTracks.Add(queuedTrack);
        await _db.SaveChangesAsync();

        Log.Information("✅ Track '{TrackName}' (ID: {TrackId}) queued for cup {CupId}, scheduled on {ScheduledOn}",
            track.Name, track.TrackId, cupId, (object?)scheduleOn ?? "next available date");
    }

    public async Task<List<TrackSuggestion>> SuggestTracksAsync(string cupId, string term)
    {
        term = term.Trim();

        if (term.Length < MinSuggestionTermLength)
            return [];

        var cupFilter = CupFilter(cupId);
        var tracks = await QueueableTracks().ToListAsync();

        var matches = tracks
            .Where(track => track.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(track => (Track: track, MatchesCupFilter: cupFilter.IsTrackSuitable(track)))
            .OrderByDescending(match => match.Track.Name.StartsWith(term, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(match => match.MatchesCupFilter)
            .ThenBy(match => match.Track.Name)
            .ThenBy(match => match.Track.Scene.Name)
            .Take(MaxSuggestions)
            .ToList();

        var lastRaces = await GetLastRacesAsync(matches.Select(match => match.Track.Id).ToList());

        return matches
            .Select(match => new TrackSuggestion(match.Track, match.MatchesCupFilter, lastRaces.GetValueOrDefault(match.Track.Id)))
            .ToList();
    }

    // Cancelled competitions were replaced by a track change or a restart, so they don't count as raced
    private async Task<Dictionary<string, LastRace>> GetLastRacesAsync(List<string> trackIds)
    {
        var races = await _db.Competitions
            .Where(competition => trackIds.Contains(competition.TrackId) && competition.State != CompetitionState.Cancelled)
            .Select(competition => new { competition.TrackId, competition.StartedOn, competition.CupId })
            .ToListAsync();

        return races
            .GroupBy(race => race.TrackId)
            .Select(group => group.MaxBy(race => race.StartedOn)!)
            .ToDictionary(
                race => race.TrackId,
                race => new LastRace(race.StartedOn, race.CupId, TrackService.IsInReuseCooldown(race.StartedOn)));
    }

    // Admins may queue any available track: the cup filter only guides random picks
    private IQueryable<Track> QueueableTracks() => _db.Tracks.Available().Include(t => t.Scene);

    private TrackFilter CupFilter(string cupId) => new(_cupService.GetCupOptions(cupId).TrackFilter);

    public async Task<QueuedTrack?> TryDequeueNextTrackAsync(string cupId)
    {
        var today = DateTime.Today;

        var nextTrack = await _db.QueuedTracks
            .ForCup(cupId)
            .NotUsed()
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .FirstOrDefaultAsync(x => x.ScheduledOn == today);

        nextTrack ??= await _db.QueuedTracks
            .ForCup(cupId)
            .NotUsed()
            .Where(x => x.ScheduledOn == null)
            .OrderBy(x => x.AddedOn)
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .FirstOrDefaultAsync();

        if (nextTrack is null)
        {
            Log.Debug("No queued tracks available for cup {CupId}", cupId);
            return null;
        }

        nextTrack.Used = true;
        await _db.SaveChangesAsync();

        Log.Information("Picked queued track '{TrackName}' for cup {CupId} (scheduled: {WasScheduled})",
            nextTrack.Track.Name, cupId, nextTrack.ScheduledOn.HasValue);

        return nextTrack;
    }

    public async Task<List<QueuedTrack>> GetQueueAsync(string cupId)
    {
        return await _db.QueuedTracks
            .ForCup(cupId)
            .NotUsed()
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .OrderBy(x => x.ScheduledOn == null ? DateTime.MaxValue : x.ScheduledOn)
            .ThenBy(x => x.AddedOn)
            .ToListAsync();
    }

    public async Task RemoveFromQueueAsync(Guid id)
    {
        Log.Information("Removing queued track {Id} from queue", id);
        var queuedTrack = await _db.QueuedTracks.FindAsync(id)
            ?? throw new NullReferenceException($"Record with id: {id} does not exist");

        _db.QueuedTracks.Remove(queuedTrack);
        await _db.SaveChangesAsync();
    }

    private bool AlreadyScheduledOnTheDate(string cupId, DateTime date)
    {
        return _db.QueuedTracks.ForCup(cupId).Any(x => x.ScheduledOn == date);
    }
}

public static class TrackQueueExtensions
{
    extension(IQueryable<QueuedTrack> query)
    {
        public IQueryable<QueuedTrack> ForCup(string cupId)
        {
            return query.Where(x => x.CupId == cupId);
        }

        public IQueryable<QueuedTrack> NotUsed()
        {
            return query.Where(x => !x.Used);
        }
    }
}
