using FluentAssertions;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Features.Tracks.Selection;
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
    public async Task ScheduleAsync_ShouldRefuse_WhenTrackIsNotAvailable(int velocidroneTrackId)
    {
        var result = await ScheduleAsAdmin(StoredId(velocidroneTrackId));

        result.Error.Should().Be(TrackSchedulingMessages.TrackNotFound);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldRefuse_WhenTrackIdIsUnknown()
    {
        var result = await ScheduleAsAdmin("unknown");

        result.Error.Should().Be(TrackSchedulingMessages.TrackNotFound);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldQueueThePickedTrack_WhenAnotherTrackSharesItsName()
    {
        var otherScene = new Scene { SceneId = 3, Name = "Other" };
        _db.Set<Track>().Add(new Track { TrackId = 104, Name = "Available", Scene = otherScene });
        _db.SaveChanges();

        await ScheduleAsAdmin(StoredId(104));

        _db.Set<QueuedTrack>().AsNoTracking().Select(q => q.Track.TrackId).Should().Equal(104);
    }

    [Fact]
    public async Task ScheduleAsync_ShouldQueueTheTrackForAnAdmin_WhenItIsOutsideTheCupFilter()
    {
        var service = SchedulingService(new TrackFilterOptions { BlacklistPatterns = ["Available"] });

        await service.ScheduleAsync(new ScheduleTrackRequest(CupId, StoredId(101), QuadId: null), SchedulingRules.Admin);

        _db.Set<QueuedTrack>().AsNoTracking().Select(q => q.Track.TrackId).Should().Equal(101);
    }

    [Fact]
    public async Task SuggestAsync_ShouldMatchAnywhereIgnoringCase_WithPrefixMatchesFirst()
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().AddRange(
            new Track { TrackId = 104, Name = "Hangar Race", Scene = scene },
            new Track { TrackId = 105, Name = "Race Hangar", Scene = scene });
        _db.SaveChanges();

        var suggestions = await SchedulingService().SuggestAsync(CupId, " RACE ", SchedulingRules.Admin);

        suggestions.Select(s => s.Track.Name).Should().Equal("Race Hangar", "Hangar Race");
    }

    [Theory]
    [InlineData("0%", "100% Speed")]
    [InlineData("e_1", "Race_1")]
    public async Task SuggestAsync_ShouldMatchWildcardCharactersLiterally(string term, string expected)
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().AddRange(
            new Track { TrackId = 104, Name = "100% Speed", Scene = scene },
            new Track { TrackId = 105, Name = "1000 Speed", Scene = scene },
            new Track { TrackId = 106, Name = "Race_1", Scene = scene },
            new Track { TrackId = 107, Name = "Race11", Scene = scene });
        _db.SaveChanges();

        var suggestions = await SchedulingService().SuggestAsync(CupId, term, SchedulingRules.Admin);

        suggestions.Select(s => s.Track.Name).Should().Equal(expected);
    }

    [Fact]
    public async Task SuggestAsync_ShouldSkipDeletedTracksAndTracksOnDeletedScenes()
    {
        var suggestions = await SchedulingService().SuggestAsync(CupId, "deleted", SchedulingRules.Admin);

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ShouldMarkTracksOutsideTheCupFilter()
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().Add(new Track { TrackId = 104, Name = "Available Freestyle", Scene = scene });
        _db.SaveChanges();
        var service = SchedulingService(new TrackFilterOptions { BlacklistPatterns = ["Freestyle"] });

        var suggestions = await service.SuggestAsync(CupId, "avail", SchedulingRules.Admin);

        suggestions.Select(s => (s.Track.Name, MatchesCupFilter: s.Issues.All(issue => issue.Kind != TrackIssueKind.OutsideCupFilter)))
            .Should().Equal(("Available", true), ("Available Freestyle", false));
    }

    [Fact]
    public async Task SuggestAsync_ShouldReturnAtMostTheMaximum_WithNamesStartingWithTheTermFirst()
    {
        var scene = _db.Set<Scene>().Single(s => s.SceneId == 1);
        _db.Set<Track>().Add(new Track { TrackId = 200, Name = "A Lap", Scene = scene });
        _db.Set<Track>().AddRange(Enumerable.Range(0, TrackSearch.MaxResults + 5)
            .Select(i => new Track { TrackId = 300 + i, Name = $"Lap {i:00}", Scene = scene }));
        _db.SaveChanges();

        var suggestions = await SchedulingService().SuggestAsync(CupId, "lap", SchedulingRules.Admin);

        suggestions.Should().HaveCount(TrackSearch.MaxResults);
        suggestions.Should().OnlyContain(s => s.Track.Name.StartsWith("Lap "));
    }

    [Fact]
    public async Task SuggestAsync_ShouldHideTracksOnOtherScenesOrOfOtherTypes_ForPatrons()
    {
        var hangar = _db.Set<Scene>().Single(s => s.SceneId == 1);
        var other = new Scene { SceneId = 9, Name = "Other" };
        _db.Set<Track>().AddRange(
            new Track { TrackId = 104, Name = "Gate Race", Scene = hangar, Type = 1 },
            new Track { TrackId = 105, Name = "Gate Elsewhere", Scene = other, Type = 1 },
            new Track { TrackId = 106, Name = "Gate Freestyle", Scene = hangar, Type = 7 },
            new Track { TrackId = 107, Name = "Gate Unknown Type", Scene = hangar });
        _db.SaveChanges();
        var service = SchedulingService(new TrackFilterOptions { WhitelistScenes = { [1] = "Hangar" }, BlacklistTrackTypes = [7] });

        var suggestions = await service.SuggestAsync(CupId, "gate", SchedulingRules.ForPatron(new ApplicationUser()));

        suggestions.Select(s => s.Track.Name).Should().Equal("Gate Race");
    }

    [Fact]
    public async Task SuggestAsync_ShouldSuggestNothing_WhenTermIsShorterThanTheMinimum()
    {
        var suggestions = await SchedulingService().SuggestAsync(CupId, "A", SchedulingRules.Admin);

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ShouldReportTheLatestRace_IgnoringCancelledCompetitions()
    {
        var track = _db.Set<Track>().Single(t => t.TrackId == 101);
        var raced = DateTime.Now.AddMonths(-8);
        _db.Set<Competition>().AddRange(
            new Competition { CupId = "whoop-class", Track = track, State = CompetitionState.Closed, StartedOn = raced.AddDays(-30) },
            new Competition { CupId = "whoop-class", Track = track, State = CompetitionState.Closed, StartedOn = raced },
            new Competition { CupId = CupId, Track = track, State = CompetitionState.Cancelled, StartedOn = DateTime.Now.AddDays(-1) });
        _db.SaveChanges();

        var suggestion = (await SchedulingService().SuggestAsync(CupId, "available", SchedulingRules.Admin)).Single();

        suggestion.LastRace.Should().BeEquivalentTo(new LastRace(raced, "whoop-class", IsInReuseCooldown: false));
    }

    [Fact]
    public async Task SuggestAsync_ShouldFlagTheReuseCooldown_WhenRacedInTheLastSixMonths()
    {
        var track = _db.Set<Track>().Single(t => t.TrackId == 101);
        _db.Set<Competition>().Add(new Competition { CupId = CupId, Track = track, State = CompetitionState.Closed, StartedOn = DateTime.Now.AddMonths(-2) });
        _db.SaveChanges();

        var suggestion = (await SchedulingService().SuggestAsync(CupId, "available", SchedulingRules.Admin)).Single();

        suggestion.LastRace!.IsInReuseCooldown.Should().BeTrue();
    }

    [Fact]
    public async Task SuggestAsync_ShouldReportNoRace_WhenTheTrackWasNeverRaced()
    {
        var suggestion = (await SchedulingService().SuggestAsync(CupId, "available", SchedulingRules.Admin)).Single();

        suggestion.LastRace.Should().BeNull();
    }

    private TrackService TrackService() => new(_db);

    private string StoredId(int velocidroneTrackId) => _db.Set<Track>().Single(t => t.TrackId == velocidroneTrackId).Id;

    private TrackSchedulingService SchedulingService(TrackFilterOptions? trackFilter = null)
    {
        var cup = new CupOptions { IsEnabled = true, TrackFilter = trackFilter ?? new TrackFilterOptions() };
        var cups = new CupsConfiguration { Definitions = { [CupId] = cup } };
        var quotaPolicy = new PatronQuotaPolicy(_db, Options.Create(new TrackSchedulingOptions()));
        return new TrackSchedulingService(_db, new CupService(Options.Create(cups)), new TrackQueueService(_db), quotaPolicy);
    }

    private Task<ScheduleTrackResult> ScheduleAsAdmin(string trackId) =>
        SchedulingService().ScheduleAsync(new ScheduleTrackRequest(CupId, trackId, QuadId: null), SchedulingRules.Admin);
}
