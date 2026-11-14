using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API.Velocidrone;
using Veloci.Logic.API.Velocidrone.Dto;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

public class TrackService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackService>();
    private const double MinimumGoodRating = 3;

    private readonly IRepository<Track> _tracks;
    private readonly IRepository<TrackMap> _maps;
    private readonly IRepository<Competition> _competitions;
    private readonly VelocidroneClient _velocidrone;
    private List<string>? _usedTrackIds;

    public TrackService(
        IRepository<Track> tracks,
        IRepository<TrackMap> maps,
        IRepository<Competition> competitions,
        VelocidroneClient velocidrone)
    {
        _tracks = tracks;
        _maps = maps;
        _competitions = competitions;
        _velocidrone = velocidrone;
    }

    public async Task<List<ParsedTrackModel>> GetSuitableTracksAsync(TrackFilter trackFilter)
    {
        var tracks = await _velocidrone.TracksAsync();
        var maps = tracks
            .GroupBy(t => t.scenery_id)
            .Select(ToMap)
            .ToList();

        Log.Debug("Fetched {MapCount} maps from Velocidrone", maps.Count);

        var suitableTracks = trackFilter.GetSuitableTracks(maps);
        Log.Debug("Filtered to {FilteredCount} suitable tracks", suitableTracks.Count);

        return suitableTracks;
    }

    public async Task<Track> GetRandomTrackAsync(IReadOnlyList<ParsedTrackModel> filteredTracks)
    {
        Log.Information("🎯 Starting track selection process");

        var usedTrackIds = await GetUsedTrackIdsAsync();

        Log.Information("Found {FilteredCount} candidate tracks for 5-inch racing, excluding {UsedTrackCount} recently used tracks",
            filteredTracks.Count, usedTrackIds.Count);

        var attempts = 0;
        while (true)
        {
            attempts++;
            var track = GetRandomElement(filteredTracks);
            var dbTrack = await GetOrCreateTrackAsync(track.Map.Name, track.Map.Id, track.Name, track.Id);

            Log.Debug("Track selection attempt {Attempt}: Evaluating {TrackName} (ID: {TrackId}) - Rating: {Rating}, Recently used: {RecentlyUsed}",
                attempts, dbTrack.Name, dbTrack.TrackId, dbTrack.Rating?.Value, usedTrackIds.Contains(dbTrack.Id));

            if (dbTrack.Rating?.Value is null or >= MinimumGoodRating && !usedTrackIds.Contains(dbTrack.Id))
            {
                Log.Information("✅ Selected track {TrackName} (ID: {TrackId}) from {FilteredCount} candidates after {Attempts} attempts",
                    dbTrack.Name, dbTrack.TrackId, filteredTracks.Count, attempts);
                return dbTrack;
            }
        }
    }

    // Fallback for when Velocidrone's track list is unavailable: picks from tracks this cup already raced on.
    // Cup history stands in for the track filter, since track types aren't stored.
    public async Task<Track> GetRandomKnownTrackAsync(string cupId)
    {
        var usedTrackIds = await GetUsedTrackIdsAsync();

        var candidates = await _competitions
            .GetAll(c => c.CupId == cupId && c.Track.Rating != null && c.Track.Rating.Value >= MinimumGoodRating)
            .Select(c => c.Track)
            .Where(t => !usedTrackIds.Contains(t.Id))
            .Distinct()
            .ToListAsync();

        if (candidates.Count == 0)
            throw new InvalidOperationException($"No positively rated known tracks left for cup {cupId}");

        var track = GetRandomElement(candidates);
        Log.Information("✅ Selected known track {TrackName} (ID: {TrackId}) from {CandidateCount} positively rated candidates",
            track.Name, track.TrackId, candidates.Count);

        return track;
    }

    public async Task<Track> GetOrCreateTrackAsync(string mapName, int mapId, string trackName, int trackId)
    {
        var dbTrack = await _tracks
                .GetAll()
                .FirstOrDefaultAsync(t => t.TrackId == trackId);

        return dbTrack ?? await CreateNewTrackAsync(mapName, mapId, trackName, trackId);
    }

    private async Task<Track> CreateNewTrackAsync(string mapName, int mapId, string trackName, int trackId)
    {
        Log.Debug("Creating new track {TrackName} (ID: {TrackId}) in map {MapName}", trackName, trackId, mapName);

        var dbMap = await _maps
                        .GetAll()
                        .FirstOrDefaultAsync(m => m.Name == mapName)
                    ?? await CreateNewMapAsync(mapName, mapId);

        if (dbMap.MapId == 0)
        {
            Log.Debug("Updating map {MapName} with MapId {MapId} (legacy data migration)", mapName, mapId);
            dbMap.MapId = mapId; // since MapId property was added later, some maps dont have this value
        }

        var track = new Track
        {
            MapId = dbMap.Id,
            Name = trackName,
            TrackId = trackId
        };

        await _tracks.AddAsync(track);
        await _tracks.SaveChangesAsync();
        Log.Information("✨ Created new track {TrackName} (ID: {TrackId}) in map {MapName}", trackName, trackId, mapName);

        return track;
    }

    private async Task<List<string>> GetUsedTrackIdsAsync()
    {
        if (_usedTrackIds is not null)
        {
            Log.Debug("Using cached list of {UsedTrackCount} recently used track IDs", _usedTrackIds.Count);
            return _usedTrackIds;
        }

        var start = DateTime.Now.AddMonths(-6);
        Log.Debug("Querying for tracks used since {StartDate}", start.ToString("yyyy-MM-dd"));

        var ids = await _competitions
            .GetAll(comp => comp.StartedOn > start)
            .Select(comp => comp.Track.Id)
            .ToListAsync();

        _usedTrackIds = ids;
        Log.Debug("Found {UsedTrackCount} tracks used in the last 6 months", ids.Count);

        return ids;
    }

    private async Task<TrackMap> CreateNewMapAsync(string name, int mapId)
    {
        Log.Information("Creating new map {MapName} (ID: {MapId})", name, mapId);

        var map = new TrackMap
        {
            Name = name,
            MapId = mapId
        };

        await _maps.AddAsync(map);
        await _maps.SaveChangesAsync();
        Log.Debug("Successfully created map {MapName}", name);

        return map;
    }

    private static ParsedMapModel ToMap(IGrouping<int, TrackDto> sceneTracks)
    {
        var map = new ParsedMapModel
        {
            Id = sceneTracks.Key,
            Name = sceneTracks.Key.ToString() // the API has no scene names, TrackFilter maps them from the cup's scene whitelist
        };

        map.Tracks = sceneTracks
            .Select(t => new ParsedTrackModel
            {
                Map = map,
                Id = t.track_id,
                Name = t.track_name,
                Type = t.type
            })
            .ToList();

        return map;
    }

    private T GetRandomElement<T>(IReadOnlyList<T> list)
    {
        var random = new Random();
        var randomIndex = random.Next(0, list.Count);
        Log.Debug("Selected random element at index {Index} from list of {Count} items", randomIndex, list.Count);
        return list[randomIndex];
    }
}
