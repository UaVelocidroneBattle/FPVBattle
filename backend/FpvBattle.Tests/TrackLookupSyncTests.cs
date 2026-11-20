using FluentAssertions;
using FpvBattle.Application.API.Velocidrone;
using FpvBattle.Application.API.Velocidrone.Dto;
using FpvBattle.Application.Features.Tracks.Models;
using FpvBattle.Application.Features.Tracks.Services;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Tests;

public class TrackLookupSyncTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly FakeVelocidroneClient _velocidrone = new();

    public TrackLookupSyncTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task SyncAsync_ShouldAddScenesAndTracks_WhenTheyAreNew()
    {
        _velocidrone.Scenes = [Scenery(3, "Hangar"), Scenery(7, "Industrial Wasteland")];
        _velocidrone.Tracks = [OfficialTrack(101, "Race 1", sceneId: 3, type: 1), OfficialTrack(102, "Race 2", sceneId: 7, type: 8)];

        var result = await SyncAsync();

        result.Scenes.Added.Should().Be(2);
        result.Tracks.Added.Should().Be(2);
        StoredTracks().Should().BeEquivalentTo(new[]
        {
            new { TrackId = 101, Name = "Race 1", Type = (int?)1, SceneName = "Hangar", IsDeleted = false },
            new { TrackId = 102, Name = "Race 2", Type = (int?)8, SceneName = "Industrial Wasteland", IsDeleted = false }
        });
    }

    [Fact]
    public async Task SyncAsync_ShouldUpdateNamesTypesAndScenes_WhenVelocidroneChangedThem()
    {
        _velocidrone.Scenes = [Scenery(3, "Hangar"), Scenery(7, "Industrial Wasteland")];
        _velocidrone.Tracks = [OfficialTrack(101, "Race 1", sceneId: 3, type: 1), OfficialTrack(102, "Race 2", sceneId: 7, type: 1)];
        await SyncAsync();

        _velocidrone.Scenes = [Scenery(3, "Hangar Remastered"), Scenery(7, "Industrial Wasteland")];
        _velocidrone.Tracks = [OfficialTrack(101, "Race 1 v2", sceneId: 7, type: 8), OfficialTrack(102, "Race 2", sceneId: 7, type: 1)];
        var result = await SyncAsync();

        result.Scenes.Should().BeEquivalentTo(new { Added = 0, Updated = 1, Restored = 0, Deleted = 0 });
        result.Tracks.Should().BeEquivalentTo(new { Added = 0, Updated = 1, Restored = 0, Deleted = 0 });
        StoredTracks().Single(t => t.TrackId == 101)
            .Should().BeEquivalentTo(new { Name = "Race 1 v2", Type = (int?)8, SceneName = "Industrial Wasteland" });
        StoredSceneNames().Should().Contain("Hangar Remastered");
    }

    [Fact]
    public async Task SyncAsync_ShouldSoftDeleteScenesAndTracks_WhenVelocidroneNoLongerReturnsThem()
    {
        _velocidrone.Scenes = [Scenery(1, "Kept"), Scenery(2, "Removed")];
        _velocidrone.Tracks = [OfficialTrack(101, "Kept", sceneId: 1), OfficialTrack(102, "Removed", sceneId: 2)];
        await SyncAsync();

        _velocidrone.Scenes = [Scenery(1, "Kept")];
        _velocidrone.Tracks = [OfficialTrack(101, "Kept", sceneId: 1)];
        var result = await SyncAsync();

        result.Scenes.Deleted.Should().Be(1);
        result.Tracks.Deleted.Should().Be(1);
        StoredTracks().Should().BeEquivalentTo(new[]
        {
            new { TrackId = 101, IsDeleted = false, SceneIsDeleted = false },
            new { TrackId = 102, IsDeleted = true, SceneIsDeleted = true }
        });
    }

    [Fact]
    public async Task SyncAsync_ShouldRestoreScenesAndTracks_WhenVelocidroneReturnsThemAgain()
    {
        _velocidrone.Scenes = [Scenery(1, "Kept"), Scenery(2, "Returning")];
        _velocidrone.Tracks = [OfficialTrack(101, "Kept", sceneId: 1), OfficialTrack(102, "Returning", sceneId: 2)];
        await SyncAsync();
        _velocidrone.Scenes = [Scenery(1, "Kept")];
        _velocidrone.Tracks = [OfficialTrack(101, "Kept", sceneId: 1)];
        await SyncAsync();

        _velocidrone.Scenes = [Scenery(1, "Kept"), Scenery(2, "Returning")];
        _velocidrone.Tracks = [OfficialTrack(101, "Kept", sceneId: 1), OfficialTrack(102, "Returning", sceneId: 2)];
        var result = await SyncAsync();

        result.Scenes.Restored.Should().Be(1);
        result.Tracks.Restored.Should().Be(1);
        StoredTracks().Should().OnlyContain(t => !t.IsDeleted && !t.SceneIsDeleted);
    }

    [Fact]
    public async Task SyncAsync_ShouldSkipDeletions_WhenVelocidroneReturnsLessThanHalfOfTheAvailableRows()
    {
        _velocidrone.Scenes = [Scenery(1, "A"), Scenery(2, "B"), Scenery(3, "C")];
        _velocidrone.Tracks = [OfficialTrack(101, "A", sceneId: 1), OfficialTrack(102, "B", sceneId: 2), OfficialTrack(103, "C", sceneId: 3)];
        await SyncAsync();

        _velocidrone.Scenes = [Scenery(1, "A")];
        _velocidrone.Tracks = [OfficialTrack(101, "A", sceneId: 1)];
        var result = await SyncAsync();

        result.Scenes.Deleted.Should().Be(0);
        result.Tracks.Deleted.Should().Be(0);
        StoredTracks().Should().OnlyContain(t => !t.IsDeleted && !t.SceneIsDeleted);
    }

    [Fact]
    public async Task SyncAsync_ShouldCreatePlaceholderScene_WhenTrackSceneIsMissingFromTheSceneList()
    {
        _velocidrone.Scenes = [Scenery(1, "Known")];
        _velocidrone.Tracks = [OfficialTrack(101, "Known", sceneId: 1), OfficialTrack(102, "Orphan", sceneId: 44)];

        await SyncAsync();

        StoredTracks().Single(t => t.TrackId == 102).SceneName.Should().Be("Scene 44");
    }

    [Fact]
    public async Task SyncAsync_ShouldNameThePlaceholderScene_WhenTheSceneListReturnsItLater()
    {
        _velocidrone.Scenes = [];
        _velocidrone.Tracks = [OfficialTrack(101, "Race", sceneId: 44)];
        await SyncAsync();

        _velocidrone.Scenes = [Scenery(44, "Dynamic Polyworld")];
        await SyncAsync();

        StoredTracks().Single().SceneName.Should().Be("Dynamic Polyworld");
    }

    private async Task<TrackLookupSyncResult> SyncAsync()
    {
        _db.ChangeTracker.Clear();
        return await new TrackLookupSync(_db, _velocidrone).SyncAsync(CancellationToken.None);
    }

    private List<StoredTrack> StoredTracks() => _db.Set<Track>()
        .AsNoTracking()
        .OrderBy(t => t.TrackId)
        .Select(t => new StoredTrack(t.TrackId, t.Name, t.Type, t.IsDeleted, t.Scene.Name, t.Scene.IsDeleted))
        .ToList();

    private List<string> StoredSceneNames() => _db.Set<Scene>().AsNoTracking().Select(s => s.Name).ToList();

    private static SceneryDto Scenery(int id, string name) => new() { scenery_id = id, scenery_name = name };

    private static TrackDto OfficialTrack(int id, string name, int sceneId, int type = 1) =>
        new() { track_id = id, track_name = name, scenery_id = sceneId, type = type };

    private sealed record StoredTrack(int TrackId, string Name, int? Type, bool IsDeleted, string SceneName, bool SceneIsDeleted);

    private sealed class FakeVelocidroneClient : IVelocidroneClient
    {
        public ICollection<SceneryDto> Scenes { get; set; } = [];
        public ICollection<TrackDto> Tracks { get; set; } = [];

        public Task<ICollection<SceneryDto>> ScenesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Scenes);

        public Task<ICollection<TrackDto>> TracksAsync(CancellationToken cancellationToken = default) => Task.FromResult(Tracks);

        public Task<ICollection<TrackTimeDto>> LeaderboardAsync(int trackId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ICollection<ModelDto>> ModelsAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
