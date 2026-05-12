using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Services.Tracks;

public class TrackQueueService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackQueueService>();
    private readonly ITrackFetcher _trackFetcher;
    private readonly ICupService _cupService;
    private readonly TrackService _trackService;
    private readonly IRepository<QueuedTrack> _trackQueue;
    private readonly IRepository<QuadModel> _quads;

    public TrackQueueService(
        ITrackFetcher trackFetcher,
        ICupService cupService,
        IRepository<QueuedTrack> trackQueue,
        TrackService trackService,
        IRepository<QuadModel> quads)
    {
        _trackFetcher = trackFetcher;
        _cupService = cupService;
        _trackQueue = trackQueue;
        _trackService = trackService;
        _quads = quads;
    }

    public async Task QueueTrackAsync(string cupId, string trackName, DateTime? scheduleOn, int? quadId)
    {
        Log.Information("Queuing track {TrackName} for cup {CupId}, scheduled on {ScheduledOn}",
            trackName, cupId, (object?)scheduleOn ?? "next available date");

        if (scheduleOn.HasValue && AlreadyScheduledOnTheDate(cupId, scheduleOn.Value))
        {
            throw new Exception($"There is already a track scheduled on the date {scheduleOn.Value}");
        }

        var cupOptions = _cupService.GetCupOptions(cupId);
        var trackFilter = new TrackFilter(cupOptions.TrackFilter);
        var maps = await _trackFetcher.FetchMapsAsync();
        var tracks = trackFilter.GetSuitableTracks(maps);
        var found = tracks.Where(track => track.Name == trackName).ToList();

        if (found.Count == 0)
        {
            throw new Exception($"Track {trackName} not found");
        }

        if (found.Count > 1)
        {
            throw new Exception($"Multiple tracks found with the name {trackName}");
        }

        var track = found.Single();
        var dbTrack = await _trackService.GetOrCreateTrackAsync(track.Map.Name, track.Map.Id, track.Name, track.Id);
        QuadModel? quad = null;

        if (quadId is not null)
        {
            quad = await _quads.FindAsync(quadId.Value);
        }

        var queuedTrack = new QueuedTrack
        {
            TrackId = dbTrack.Id,
            ScheduledOn = scheduleOn,
            AddedOn = DateTime.UtcNow,
            CupId = cupId,
            Quad = quad
        };

        await _trackQueue.AddAsync(queuedTrack);

        Log.Information("✅ Track '{TrackName}' (ID: {TrackId}) queued for cup {CupId}, scheduled on {ScheduledOn}",
            dbTrack.Name, dbTrack.TrackId, cupId, (object?)scheduleOn ?? "next available date");
    }

    public async Task<QueuedTrack?> TryDequeueNextTrackAsync(string cupId)
    {
        var today = DateTime.Today;

        var nextTrack = await _trackQueue.GetAll()
            .ForCup(cupId)
            .NotUsed()
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .FirstOrDefaultAsync(x => x.ScheduledOn == today);

        nextTrack ??= await _trackQueue.GetAll()
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
        await _trackQueue.SaveChangesAsync();

        Log.Information("Picked queued track '{TrackName}' for cup {CupId} (scheduled: {WasScheduled})",
            nextTrack.Track.Name, cupId, nextTrack.ScheduledOn.HasValue);

        return nextTrack;
    }

    public async Task<List<QueuedTrack>> GetQueueAsync(string cupId)
    {
        return await _trackQueue.GetAll()
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
        await _trackQueue.RemoveAsync(id);
    }

    private bool AlreadyScheduledOnTheDate(string cupId, DateTime date)
    {
        return _trackQueue.GetAll().ForCup(cupId).Any(x => x.ScheduledOn == date);
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
