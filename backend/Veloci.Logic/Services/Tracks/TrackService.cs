using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Services.Tracks.Models;

namespace Veloci.Logic.Services.Tracks;

public class TrackService
{
    private static readonly ILogger _log = Log.ForContext<TrackService>();
    
    private readonly IRepository<Track> _tracks;
    private readonly IRepository<TrackMap> _maps;
    private readonly IRepository<Competition> _competitions;
    private readonly ITrackFetcher _trackFetcher;
    private List<string>? _usedTrackIds;

    public TrackService(
        ITrackFetcher trackFetcher,
        IRepository<Track> tracks,
        IRepository<TrackMap> maps,
        IRepository<Competition> competitions)
    {
        _trackFetcher = trackFetcher;
        _tracks = tracks;
        _maps = maps;
        _competitions = competitions;
    }

    public async Task<Track> GetRandomTrackAsync()
    {
        _log.Information("🎯 Starting track selection process");
        
        var maps = await _trackFetcher.FetchMapsAsync();
        _log.Debug("Fetched {MapCount} maps from track fetcher", maps.Count());
        
        var filteredTracks = GetCandidateTracks(maps);
        var usedTrackIds = await GetUsedTrackIdsAsync();
        
        _log.Information("Found {FilteredCount} candidate tracks for 5-inch racing, excluding {UsedTrackCount} recently used tracks", 
            filteredTracks.Count, usedTrackIds.Count);

        var attempts = 0;
        while (true)
        {
            attempts++;
            var track = GetRandomElement(filteredTracks);
            var dbTrack = await GetTrackAsync(track.Id)
                          ?? await CreateNewTrackAsync(track.Map.Name, track.Map.Id, track.Name, track.Id);

            _log.Debug("Track selection attempt {Attempt}: Evaluating {TrackName} (ID: {TrackId}) - Rating: {Rating}, Recently used: {RecentlyUsed}", 
                attempts, dbTrack.Name, dbTrack.TrackId, dbTrack.Rating?.Value, usedTrackIds.Contains(dbTrack.Id));

            if (dbTrack.Rating?.Value is null or >= 0 && !usedTrackIds.Contains(dbTrack.Id))
            {
                _log.Information("✅ Selected track {TrackName} (ID: {TrackId}) from {FilteredCount} candidates after {Attempts} attempts", 
                    dbTrack.Name, dbTrack.TrackId, filteredTracks.Count, attempts);
                return dbTrack;
            }
        }
    }

    private List<ParsedTrackModel> GetCandidateTracks(IEnumerable<ParsedMapModel> maps)
    {
        var allTracks = maps.SelectMany(m => m.Tracks).ToList();
        _log.Debug("Total tracks from all maps: {TrackCount}", allTracks.Count);
        
        var trackFilter = new TrackFilter();
        var filteredTracks = allTracks.Where(t => trackFilter.IsTrackGoodFor5inchRacing(t)).ToList();
        
        _log.Debug("Filtered to {FilteredCount} tracks suitable for 5-inch racing (from {TotalCount} total)", 
            filteredTracks.Count, allTracks.Count);

        return filteredTracks;
    }

    private async Task<Track?> GetTrackAsync(int trackId)
    {
        return await _tracks
                .GetAll()
                .FirstOrDefaultAsync(t => t.TrackId == trackId);
    }

    private async Task<Track> CreateNewTrackAsync(string mapName, int mapId, string trackName, int trackId)
    {
        _log.Debug("Creating new track {TrackName} (ID: {TrackId}) in map {MapName}", trackName, trackId, mapName);
        
        var dbMap = await _maps
                        .GetAll()
                        .FirstOrDefaultAsync(m => m.Name == mapName)
                    ?? await CreateNewMapAsync(mapName, mapId);

        if (dbMap.MapId == 0)
        {
            _log.Debug("Updating map {MapName} with MapId {MapId} (legacy data migration)", mapName, mapId);
            dbMap.MapId = mapId; // since MapId property was added later, some maps dont have this value
        }

        var track = new Track
        {
            MapId = dbMap.Id,
            Name = trackName,
            TrackId = trackId
        };

        await _tracks.AddAsync(track);
        _log.Information("✨ Created new track {TrackName} (ID: {TrackId}) in map {MapName}", trackName, trackId, mapName);

        return track;
    }

    private async Task<List<string>> GetUsedTrackIdsAsync()
    {
        if (_usedTrackIds is not null)
        {
            _log.Debug("Using cached list of {UsedTrackCount} recently used track IDs", _usedTrackIds.Count);
            return _usedTrackIds;
        }

        var start = DateTime.Now.AddMonths(-6);
        _log.Debug("Querying for tracks used since {StartDate}", start.ToString("yyyy-MM-dd"));

        var ids = await _competitions
            .GetAll(comp => comp.StartedOn > start)
            .Select(comp => comp.Track.Id)
            .ToListAsync();

        _usedTrackIds = ids;
        _log.Debug("Found {UsedTrackCount} tracks used in the last 6 months", ids.Count);

        return ids;
    }

    private async Task<TrackMap> CreateNewMapAsync(string name, int mapId)
    {
        _log.Information("Creating new map {MapName} (ID: {MapId})", name, mapId);
        
        var map = new TrackMap
        {
            Name = name,
            MapId = mapId
        };

        await _maps.AddAsync(map);
        _log.Debug("Successfully created map {MapName}", name);

        return map;
    }

    private T GetRandomElement<T>(IReadOnlyList<T> list)
    {
        var random = new Random();
        var randomIndex = random.Next(0, list.Count);
        _log.Debug("Selected random element at index {Index} from list of {Count} items", randomIndex, list.Count);
        return list[randomIndex];
    }
}
