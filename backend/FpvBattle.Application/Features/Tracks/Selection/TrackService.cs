using FpvBattle.Application.Features.Tracks;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Features.Tracks.Selection;

public class TrackService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackService>();

    private readonly IApplicationDbContext _db;
    private HashSet<string>? _unpickableTrackIds;

    public TrackService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Track>> GetSuitableTracksAsync(TrackFilter trackFilter)
    {
        var tracks = await _db.Tracks
            .Available()
            .Include(t => t.Scene)
            .Include(t => t.Rating)
            .ToListAsync();

        var suitableTracks = trackFilter.GetSuitableTracks(tracks);
        Log.Debug("Filtered {TrackCount} available tracks to {SuitableCount} suitable tracks", tracks.Count, suitableTracks.Count);

        return suitableTracks;
    }

    public async Task<Track> GetRandomTrackAsync(IReadOnlyList<Track> suitableTracks)
    {
        var unpickableTrackIds = await GetUnpickableTrackIdsAsync();

        var candidates = suitableTracks
            .Where(t => TrackReuseRules.IsWellRated(t.Rating) && !unpickableTrackIds.Contains(t.Id))
            .ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException(
                $"None of the {suitableTracks.Count} suitable tracks is well rated, unused in the last {TrackReuseRules.CooldownMonths} months and not waiting in the track queue");

        var track = candidates[Random.Shared.Next(candidates.Count)];
        Log.Information("✅ Selected track {TrackName} (ID: {TrackId}) from {CandidateCount} candidates out of {SuitableCount} suitable tracks",
            track.Name, track.TrackId, candidates.Count, suitableTracks.Count);

        return track;
    }

    // A track waiting in the queue is skipped too, so it isn't raced at random before its turn comes
    private async Task<HashSet<string>> GetUnpickableTrackIdsAsync()
    {
        if (_unpickableTrackIds is not null)
            return _unpickableTrackIds;

        var since = TrackReuseRules.CooldownStart;
        var recentlyRacedIds = await _db.Competitions
            .NotCancelled()
            .Where(comp => comp.StartedOn > since)
            .Select(comp => comp.TrackId)
            .ToListAsync();

        var queuedIds = await _db.QueuedTracks
            .NotUsed()
            .Select(queued => queued.TrackId)
            .ToListAsync();

        _unpickableTrackIds = recentlyRacedIds.Concat(queuedIds).ToHashSet();
        Log.Debug("Found {RacedCount} tracks raced since {Since} and {QueuedCount} queued tracks",
            recentlyRacedIds.Count, since.ToString("dd.MM.yyyy"), queuedIds.Count);

        return _unpickableTrackIds;
    }
}
