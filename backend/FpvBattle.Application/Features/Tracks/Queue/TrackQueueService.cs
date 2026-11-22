using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Features.Tracks.Queue;

/// <summary>
/// The line of tracks waiting to become a cup's daily track. What may be added to it is decided by
/// <see cref="Scheduling.TrackSchedulingService"/>; this class only keeps the line.
/// </summary>
public class TrackQueueService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackQueueService>();
    private readonly IApplicationDbContext _db;

    public TrackQueueService(IApplicationDbContext db)
    {
        _db = db;
    }

    /// <exception cref="TrackAlreadyQueuedException">The track is already waiting in the queue of any cup.</exception>
    public async Task AddAsync(QueuedTrack entry)
    {
        if (await IsWaitingAsync(entry.TrackId))
            throw new TrackAlreadyQueuedException();

        entry.AddedOn = DateTime.UtcNow;
        _db.QueuedTracks.Add(entry);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _db.QueuedTracks.Remove(entry);

            // Another request queued the track in between, and the unique index on waiting tracks refused this copy
            if (await IsWaitingAsync(entry.TrackId))
                throw new TrackAlreadyQueuedException();

            throw;
        }

        Log.Information("✅ Track {TrackId} queued for cup {CupId}, scheduled on {ScheduledOn}, by {ScheduledBy}",
            entry.TrackId, entry.CupId, (object?)entry.ScheduledOn ?? "next available date", entry.ScheduledByUserId ?? "admin");
    }

    public Task<bool> IsDateTakenAsync(string cupId, DateTime date) =>
        _db.QueuedTracks.ForCup(cupId).AnyAsync(x => x.ScheduledOn == date);

    public async Task<QueuedTrack?> TryDequeueNextTrackAsync(string cupId)
    {
        var today = DateTime.Today;
        var waiting = _db.QueuedTracks.ForCup(cupId).NotUsed();

        var nextTrack = await waiting
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .FirstOrDefaultAsync(x => x.ScheduledOn == today);

        nextTrack ??= await waiting
            .Undated()
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

        Log.Information("Picked queued track '{TrackName}' for cup {CupId} (scheduled: {WasScheduled}, by: {ScheduledBy})",
            nextTrack.Track.Name, cupId, nextTrack.ScheduledOn.HasValue, nextTrack.ScheduledByUserId ?? "admin");

        return nextTrack;
    }

    public async Task<List<QueuedTrack>> GetQueueAsync(string cupId)
    {
        return await _db.QueuedTracks
            .ForCup(cupId)
            .NotUsed()
            .Include(x => x.Track)
            .Include(x => x.Quad)
            .Include(x => x.ScheduledBy)
            .OrderBy(x => x.ScheduledOn == null ? DateTime.MaxValue : x.ScheduledOn)
            .ThenBy(x => x.AddedOn)
            .ToListAsync();
    }

    // Admin tracks dated for a day jump the line on that day, so the place is a lower bound of the wait
    public async Task<QueuePlace> GetPlaceAsync(QueuedTrack entry)
    {
        var position = await _db.QueuedTracks
            .ForCup(entry.CupId)
            .NotUsed()
            .Undated()
            .CountAsync(q => q.AddedOn <= entry.AddedOn);

        return new QueuePlace(entry, position);
    }

    public async Task RemoveAsync(QueuedTrack entry)
    {
        Log.Information("Removing queued track {Id} ({TrackId}) from the {CupId} queue", entry.Id, entry.TrackId, entry.CupId);

        _db.QueuedTracks.Remove(entry);
        await _db.SaveChangesAsync();
    }

    private Task<bool> IsWaitingAsync(string trackId) =>
        _db.QueuedTracks.NotUsed().AnyAsync(q => q.TrackId == trackId);
}
