using FpvBattle.Application.API.Velocidrone;
using FpvBattle.Application.API.Velocidrone.Dto;
using FpvBattle.Application.Features.Tracks.Models;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Features.Tracks.Services;

/// <summary>
/// Mirrors Velocidrone's scenes and official tracks into the database, the only place track picks read from.
/// Rows that disappear from Velocidrone are soft deleted, because competitions and the track queue keep pointing at them.
/// </summary>
public class TrackLookupSync
{
    private const double MinimumReturnedShare = 0.5;

    private static readonly ILogger Log = Serilog.Log.ForContext<TrackLookupSync>();

    private readonly IApplicationDbContext _db;
    private readonly IVelocidroneClient _velocidrone;

    public TrackLookupSync(IApplicationDbContext db, IVelocidroneClient velocidrone)
    {
        _db = db;
        _velocidrone = velocidrone;
    }

    public async Task<TrackLookupSyncResult> SyncAsync(CancellationToken ct)
    {
        var sceneries = (await _velocidrone.ScenesAsync(ct)).DistinctBy(s => s.scenery_id).ToList();
        var officialTracks = (await _velocidrone.TracksAsync(ct)).DistinctBy(t => t.track_id).ToList();

        var scenes = new LookupIndex<Scene>(await _db.Scenes.ToListAsync(ct), s => s.SceneId);
        var tracks = new LookupIndex<Track>(await _db.Tracks.ToListAsync(ct), t => t.TrackId);

        foreach (var scenery in sceneries)
            UpsertScene(scenes, scenery.scenery_id, scenery.scenery_name);

        foreach (var officialTrack in officialTracks)
            UpsertTrack(tracks, officialTrack, SceneOf(scenes, officialTrack.scenery_id));

        DeleteNotReturned(scenes, sceneries.Count, "scenes");
        DeleteNotReturned(tracks, officialTracks.Count, "tracks");

        await _db.SaveChangesAsync(ct);

        LogCounts("Scenes", scenes.Counts);
        LogCounts("Tracks", tracks.Counts);

        return new TrackLookupSyncResult(scenes.Counts, tracks.Counts);
    }

    private Scene UpsertScene(LookupIndex<Scene> scenes, int velocidroneId, string name)
    {
        if (scenes.Find(velocidroneId) is not { } scene)
        {
            scene = new Scene { SceneId = velocidroneId, Name = name };
            _db.Scenes.Add(scene);
            scenes.Added(velocidroneId, scene);
            return scene;
        }

        var changed = scene.Name != name;
        scene.Name = name;
        scenes.Returned(velocidroneId, scene, changed);

        return scene;
    }

    private Scene SceneOf(LookupIndex<Scene> scenes, int velocidroneId)
    {
        if (scenes.WasReturned(velocidroneId))
            return scenes.Find(velocidroneId)!;

        Log.Warning("Velocidrone has official tracks on scene {SceneId}, which its scene list does not return", velocidroneId);

        var name = scenes.Find(velocidroneId)?.Name ?? $"Scene {velocidroneId}";
        return UpsertScene(scenes, velocidroneId, name);
    }

    private void UpsertTrack(LookupIndex<Track> tracks, TrackDto officialTrack, Scene scene)
    {
        if (tracks.Find(officialTrack.track_id) is not { } track)
        {
            track = new Track
            {
                TrackId = officialTrack.track_id,
                Name = officialTrack.track_name,
                Type = officialTrack.type,
                Scene = scene
            };
            _db.Tracks.Add(track);
            tracks.Added(track.TrackId, track);
            return;
        }

        var changed = track.Name != officialTrack.track_name
                      || track.Type != officialTrack.type
                      || track.SceneId != scene.Id;

        track.Name = officialTrack.track_name;
        track.Type = officialTrack.type;
        track.Scene = scene;
        tracks.Returned(track.TrackId, track, changed);
    }

    private static void DeleteNotReturned<T>(LookupIndex<T> lookup, int returnedCount, string kind)
        where T : class, ISoftDeletable
    {
        if (returnedCount < lookup.AvailableBefore * MinimumReturnedShare)
        {
            Log.Warning("Velocidrone returned only {ReturnedCount} {Kind} while {AvailableCount} are available, skipping deletions",
                returnedCount, kind, lookup.AvailableBefore);
            return;
        }

        foreach (var row in lookup.AvailableButNotReturned())
            lookup.Deleted(row);
    }

    private static void LogCounts(string kind, LookupSyncCounts counts) =>
        Log.Information("{Kind} synced from Velocidrone: {Added} added, {Updated} updated, {Restored} restored, {Deleted} deleted",
            kind, counts.Added, counts.Updated, counts.Restored, counts.Deleted);
}
