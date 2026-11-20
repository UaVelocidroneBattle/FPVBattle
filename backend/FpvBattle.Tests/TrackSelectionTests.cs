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
    [InlineData(102)]
    [InlineData(103)]
    public async Task QueueTrackAsync_ShouldRefuse_WhenTrackIsNotAvailable(int velocidroneTrackId)
    {
        var queue = () => QueueService().QueueTrackAsync(CupId, StoredId(velocidroneTrackId), scheduleOn: null, quadId: null);

        await queue.Should().ThrowAsync<Exception>().WithMessage("The track was not found or is no longer available");
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldRefuse_WhenTrackIdIsUnknown()
    {
        var queue = () => QueueService().QueueTrackAsync(CupId, "unknown", scheduleOn: null, quadId: null);

        await queue.Should().ThrowAsync<Exception>().WithMessage("The track was not found or is no longer available");
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldQueueThePickedTrack_WhenAnotherTrackSharesItsName()
    {
        var otherScene = new Scene { SceneId = 3, Name = "Other" };
        _db.Set<Track>().Add(new Track { TrackId = 104, Name = "Available", Scene = otherScene });
        _db.SaveChanges();

        await QueueService().QueueTrackAsync(CupId, StoredId(104), scheduleOn: null, quadId: null);

        _db.Set<QueuedTrack>().AsNoTracking().Select(q => q.Track.TrackId).Should().Equal(104);
    }

    [Fact]
    public async Task QueueTrackAsync_ShouldQueueTheTrack_WhenItIsOutsideTheCupFilter()
    {
        var service = QueueService(new TrackFilterOptions { BlacklistPatterns = ["Available"] });

        await service.QueueTrackAsync(CupId, StoredId(101), scheduleOn: null, quadId: null);

        _db.Set<QueuedTrack>().AsNoTracking().Select(q => q.Track.TrackId).Should().Equal(101);
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldMatchAnywhereIgnoringCase_WithPrefixMatchesFirst()
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().AddRange(
            new Track { TrackId = 104, Name = "Hangar Race", Scene = scene },
            new Track { TrackId = 105, Name = "Race Hangar", Scene = scene });
        _db.SaveChanges();

        var suggestions = await QueueService().SuggestTracksAsync(CupId, " RACE ");

        suggestions.Select(s => s.Track.Name).Should().Equal("Race Hangar", "Hangar Race");
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldSkipDeletedTracksAndTracksOnDeletedScenes()
    {
        var suggestions = await QueueService().SuggestTracksAsync(CupId, "deleted");

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldMarkTracksOutsideTheCupFilter()
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().Add(new Track { TrackId = 104, Name = "Available Freestyle", Scene = scene });
        _db.SaveChanges();
        var service = QueueService(new TrackFilterOptions { BlacklistPatterns = ["Freestyle"] });

        var suggestions = await service.SuggestTracksAsync(CupId, "avail");

        suggestions.Select(s => (s.Track.Name, s.MatchesCupFilter))
            .Should().Equal(("Available", true), ("Available Freestyle", false));
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldSuggestNothing_WhenTermIsShorterThanTheMinimum()
    {
        var suggestions = await QueueService().SuggestTracksAsync(CupId, "A");

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldReportTheLatestRace_IgnoringCancelledCompetitions()
    {
        var track = _db.Set<Track>().Single(t => t.TrackId == 101);
        var raced = DateTime.Now.AddMonths(-8);
        _db.Set<Competition>().AddRange(
            new Competition { CupId = "whoop-class", Track = track, State = CompetitionState.Closed, StartedOn = raced.AddDays(-30) },
            new Competition { CupId = "whoop-class", Track = track, State = CompetitionState.Closed, StartedOn = raced },
            new Competition { CupId = CupId, Track = track, State = CompetitionState.Cancelled, StartedOn = DateTime.Now.AddDays(-1) });
        _db.SaveChanges();

        var suggestion = (await QueueService().SuggestTracksAsync(CupId, "available")).Single();

        suggestion.LastRace.Should().BeEquivalentTo(new LastRace(raced, "whoop-class", IsInReuseCooldown: false));
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldFlagTheReuseCooldown_WhenRacedInTheLastSixMonths()
    {
        var track = _db.Set<Track>().Single(t => t.TrackId == 101);
        _db.Set<Competition>().Add(new Competition { CupId = CupId, Track = track, State = CompetitionState.Closed, StartedOn = DateTime.Now.AddMonths(-2) });
        _db.SaveChanges();

        var suggestion = (await QueueService().SuggestTracksAsync(CupId, "available")).Single();

        suggestion.LastRace!.IsInReuseCooldown.Should().BeTrue();
    }

    [Fact]
    public async Task SuggestTracksAsync_ShouldReportNoRace_WhenTheTrackWasNeverRaced()
    {
        var suggestion = (await QueueService().SuggestTracksAsync(CupId, "available")).Single();

        suggestion.LastRace.Should().BeNull();
    }

    private TrackService TrackService() => new(_db);

    private string StoredId(int velocidroneTrackId) => _db.Set<Track>().Single(t => t.TrackId == velocidroneTrackId).Id;

    private TrackQueueService QueueService(TrackFilterOptions? trackFilter = null)
    {
        var cup = new CupOptions { IsEnabled = true, TrackFilter = trackFilter ?? new TrackFilterOptions() };
        var cups = new CupsConfiguration { Definitions = { [CupId] = cup } };
        return new TrackQueueService(new CupService(Options.Create(cups)), _db);
    }
}
