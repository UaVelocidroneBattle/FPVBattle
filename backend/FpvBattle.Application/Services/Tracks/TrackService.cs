using FpvBattle.Application.Features.Tracks;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Services.Tracks;

public class TrackService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackService>();
    private const double MinimumGoodRating = 3;
    private const int ReuseCooldownMonths = 6;

    private readonly IApplicationDbContext _db;
    private HashSet<string>? _recentlyUsedTrackIds;

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
        var recentlyUsedTrackIds = await GetRecentlyUsedTrackIdsAsync();

        var candidates = suitableTracks
            .Where(t => t.Rating?.Value is null or >= MinimumGoodRating && !recentlyUsedTrackIds.Contains(t.Id))
            .ToList();

        if (candidates.Count == 0)
            throw new InvalidOperationException(
                $"None of the {suitableTracks.Count} suitable tracks is well rated and unused in the last {ReuseCooldownMonths} months");

        var track = candidates[Random.Shared.Next(candidates.Count)];
        Log.Information("✅ Selected track {TrackName} (ID: {TrackId}) from {CandidateCount} candidates out of {SuitableCount} suitable tracks",
            track.Name, track.TrackId, candidates.Count, suitableTracks.Count);

        return track;
    }

    private async Task<HashSet<string>> GetRecentlyUsedTrackIdsAsync()
    {
        if (_recentlyUsedTrackIds is not null)
            return _recentlyUsedTrackIds;

        var since = DateTime.Now.AddMonths(-ReuseCooldownMonths);
        var ids = await _db.Competitions
            .Where(comp => comp.StartedOn > since)
            .Select(comp => comp.TrackId)
            .ToListAsync();

        _recentlyUsedTrackIds = ids.ToHashSet();
        Log.Debug("Found {UsedTrackCount} tracks used since {Since}", _recentlyUsedTrackIds.Count, since.ToString("dd.MM.yyyy"));

        return _recentlyUsedTrackIds;
    }
}
