using FluentAssertions;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Services.Tracks;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Tests;

public class TrackSelectionTests : IDisposable
{
    private const string CupId = "open-class";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly TrackFilter _noFilter = new(new TrackFilterOptions());

    public TrackSelectionTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();

        var scene = new Scene { SceneId = 1, Name = "Hangar" };
        var deletedScene = new Scene { SceneId = 2, Name = "Gone", IsDeleted = true };
        _db.Set<Track>().AddRange(
            new Track { TrackId = 101, Name = "Available", Scene = scene },
            new Track { TrackId = 102, Name = "Deleted", Scene = scene, IsDeleted = true },
            new Track { TrackId = 103, Name = "On deleted scene", Scene = deletedScene });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetSuitableTracksAsync_ShouldSkipDeletedTracksAndTracksOnDeletedScenes()
    {
        var tracks = await TrackService().GetSuitableTracksAsync(_noFilter);

        tracks.Select(t => t.Name).Should().Equal("Available");
    }

    [Fact]
    public async Task GetRandomTrackAsync_ShouldNeverPickDeletedTracks()
    {
        var service = TrackService();
        var suitableTracks = await service.GetSuitableTracksAsync(_noFilter);

        for (var i = 0; i < 20; i++)
            (await service.GetRandomTrackAsync(suitableTracks)).Name.Should().Be("Available");
    }

    [Theory]
    [InlineData("Deleted")]
    [InlineData("On deleted scene")]
    public async Task QueueTrackAsync_ShouldRefuse_WhenTrackIsNotAvailable(string trackName)
    {
        var queue = () => QueueService().QueueTrackAsync(CupId, trackName, scheduleOn: null, quadId: null);

        await queue.Should().ThrowAsync<Exception>().WithMessage($"Track {trackName} not found");
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldQueueTheStoredTrack_WhenItIsAvailable()
    {
        await QueueService().QueueTrackAsync(CupId, " Available ", scheduleOn: null, quadId: null);

        _db.Set<QueuedTrack>().AsNoTracking().Select(q => q.Track.TrackId).Should().Equal(101);
    }

    private TrackService TrackService() => new(_db);

    private TrackQueueService QueueService()
    {
        var cups = new CupsConfiguration { Definitions = { [CupId] = new CupOptions { IsEnabled = true } } };
        return new TrackQueueService(new CupService(Options.Create(cups)), _db, TrackService());
    }
}
