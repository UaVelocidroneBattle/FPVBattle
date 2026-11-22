using System.Security.Claims;
using FluentAssertions;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Features.Tracks.Selection;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Web.Controllers.Api.TrackScheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Tests;

public class TrackSchedulingTests : IDisposable
{
    private const string OpenClass = "open-class";
    private const string WhoopClass = "whoop-class";
    private const int TbsSpec = 1;
    private const int TinyTrainer = 2;
    private const int NotInPool = 3;

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CupService _cupService;
    private readonly TrackSchedulingOptions _options = new()
    {
        Enabled = true,
        MonthlyLimitByTier = { ["Level 1"] = 1, ["Level 2"] = 2 }
    };
    private readonly Scene _scene = new() { SceneId = 1, Name = "Hangar" };
    private int _nextPilotId = 1;
    private int _nextTrackId = 1;

    public TrackSchedulingTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();

        _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(_db), null!, null!, null!, null!, null!, null!, null!, null!);

        var cups = new CupsConfiguration
        {
            Definitions =
            {
                [OpenClass] = new CupOptions
                {
                    Name = "Open Class",
                    IsEnabled = true,
                    TrackFilter = new TrackFilterOptions { BlacklistPatterns = ["Freestyle"] },
                    QuadOfTheDay = new QuadOfTheDayOptions { Enabled = true, Quads = ["Tiny Trainer", "TBS Spec"] }
                },
                [WhoopClass] = new CupOptions
                {
                    Name = "Whoop Class",
                    IsEnabled = true,
                    QuadOfTheDay = new QuadOfTheDayOptions { Enabled = false, Quads = ["TBS Spec"] }
                },
                ["retired-cup"] = new CupOptions { Name = "Retired", IsEnabled = false }
            }
        };
        _cupService = new CupService(Options.Create(cups));

        _db.Set<QuadModel>().AddRange(
            new QuadModel { Id = TbsSpec, Name = "TBS Spec" },
            new QuadModel { Id = TinyTrainer, Name = "Tiny Trainer" },
            new QuadModel { Id = NotInPool, Name = "Some Other Quad" });
        _db.Set<Scene>().Add(_scene);
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Overview_ShouldAskToSignIn_WhenTheUserIsAVisitor()
    {
        var overview = (await Controller(null).Overview()).Value!;

        overview.BlockedReason.Should().Be(TrackSchedulingMessages.SignIn);
        overview.MyTracks.Should().BeEmpty();
    }

    [Fact]
    public async Task Overview_ShouldListEnabledCups_WithTheirQuadPoolInConfigOrder()
    {
        var overview = (await Controller(null).Overview()).Value!;

        overview.Cups.Select(c => (c.CupId, Quads: string.Join(",", c.Quads.Select(q => q.Name))))
            .Should().Equal((OpenClass, "Tiny Trainer,TBS Spec"), (WhoopClass, ""));
    }

    [Fact]
    public async Task Overview_ShouldAskToLinkAPilot_WhenTheUserHasNone()
    {
        var user = AddUser("nopilot", withPilot: false);

        (await Controller(user).Overview()).Value!.BlockedReason.Should().Be(TrackSchedulingMessages.PilotNotLinked);
    }

    [Theory]
    [InlineData(PatreonSupporter.ActivePatronStatus, 0)]
    [InlineData("former_patron", 5)]
    public async Task Overview_ShouldSayItIsForPatrons_WhenTheUserIsNotAPayingPatron(string status, decimal amount)
    {
        var user = AddUser("free");
        AddSupporter(user, "Level 1", status, amount);

        (await Controller(user).Overview()).Value!.BlockedReason.Should().Be(TrackSchedulingMessages.PatronsOnly);
    }

    [Fact]
    public async Task Overview_ShouldSayTheTierIsNotIncluded_WhenTheTierHasNoLimitInConfig()
    {
        var user = AddUser("lowtier");
        AddSupporter(user, "Level 0");

        (await Controller(user).Overview()).Value!.BlockedReason.Should().Be(TrackSchedulingMessages.TierNotIncluded);
    }

    [Fact]
    public async Task Overview_ShouldReportTheQuota_WhenTheUserIsAPayingPatron()
    {
        var patron = AddPatron("patron", "Level 2");

        var overview = (await Controller(patron).Overview()).Value!;

        overview.BlockedReason.Should().BeNull();
        (overview.MonthlyLimit, overview.UsedThisMonth, overview.Remaining).Should().Be((2, 0, 2));
    }

    [Fact]
    public async Task Schedule_ShouldQueueTheTrackForThePatron_AtTheBackOfTheLine()
    {
        var patron = AddPatron("patron", "Level 2");
        await QueueAsAdmin(OpenClass, AddTrack("Admin pick"));
        var track = AddTrack("Patron pick");

        var scheduled = (await Controller(patron).Schedule(Request(track, TbsSpec))).Value!;

        (scheduled.TrackName, scheduled.QuadName, scheduled.QueuePosition).Should().Be(("Patron pick", "TBS Spec", 2));
        _db.Set<QueuedTrack>().AsNoTracking().Single(q => q.TrackId == track)
            .Should().Match<QueuedTrack>(q => q.ScheduledByUserId == patron.Id && q.ScheduledOn == null);
    }

    [Fact]
    public async Task Schedule_ShouldStoreNoQuad_WhenAnyQuadIsPicked()
    {
        var patron = AddPatron("patron");
        var track = AddTrack("Any quad");

        await Controller(patron).Schedule(Request(track, quadId: null));

        _db.Set<QueuedTrack>().AsNoTracking().Single().QuadId.Should().BeNull();
    }

    [Theory]
    [InlineData(OpenClass, NotInPool)]
    [InlineData(WhoopClass, TbsSpec)]
    public async Task Schedule_ShouldRefuse_WhenTheQuadIsNotInTheCupPool(string cupId, int quadId)
    {
        var patron = AddPatron("patron");

        var result = await Controller(patron).Schedule(Request(AddTrack("Track"), quadId, cupId));

        ErrorOf(result).Should().Be(TrackSchedulingMessages.QuadNotAllowed);
    }

    [Fact]
    public async Task Schedule_ShouldRefuse_WhenTheCupIsDisabled()
    {
        var patron = AddPatron("patron");

        var result = await Controller(patron).Schedule(Request(AddTrack("Track"), cupId: "retired-cup"));

        ErrorOf(result).Should().Be(TrackSchedulingMessages.CupNotAvailable);
    }

    [Fact]
    public async Task Schedule_ShouldRefuse_WhenTheTrackIsOutsideTheCupFilter()
    {
        var patron = AddPatron("patron");

        var result = await Controller(patron).Schedule(Request(AddTrack("Freestyle park")));

        ErrorOf(result).Should().Be(TrackSchedulingMessages.OutsideCupFilter);
    }

    [Fact]
    public async Task Schedule_ShouldRefuse_WhenTheTrackIsAlreadyQueuedInAnyCup()
    {
        var patron = AddPatron("patron");
        var track = AddTrack("Popular");
        await QueueAsAdmin(WhoopClass, track);

        var result = await Controller(patron).Schedule(Request(track));

        ErrorOf(result).Should().Be(TrackSchedulingMessages.AlreadyScheduled);
    }

    [Fact]
    public async Task Schedule_ShouldRefuse_WhenTheMonthlyLimitIsReached_UntilATrackIsCancelled()
    {
        var patron = AddPatron("patron", "Level 1");
        var first = (await Controller(patron).Schedule(Request(AddTrack("First")))).Value!;

        var second = await Controller(patron).Schedule(Request(AddTrack("Second")));
        ErrorOf(second).Should().StartWith("You've scheduled all 1 tracks for this month");

        (await Controller(patron).Cancel(new QueuedTrackReferenceModel { Id = first.Id })).Should().BeOfType<NoContentResult>();
        (await Controller(patron).Schedule(Request(AddTrack("Third")))).Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Schedule_ShouldNotCountTracksAddedInEarlierMonths()
    {
        var patron = AddPatron("patron", "Level 1");
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        _db.Set<QueuedTrack>().Add(new QueuedTrack
        {
            CupId = OpenClass, TrackId = AddTrack("Last month"), AddedOn = monthStart.AddDays(-1), Used = true, ScheduledByUserId = patron.Id
        });
        _db.SaveChanges();

        (await Controller(patron).Schedule(Request(AddTrack("This month")))).Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Cancel_ShouldRefuse_WhenTheTrackBelongsToAnotherUser_OrWasRaced()
    {
        var patron = AddPatron("patron");
        var other = AddPatron("other");
        var scheduled = (await Controller(patron).Schedule(Request(AddTrack("Mine")))).Value!;

        (await Controller(other).Cancel(new QueuedTrackReferenceModel { Id = scheduled.Id })).Should().BeOfType<NotFoundObjectResult>();

        await Queue().TryDequeueNextTrackAsync(OpenClass);
        (await Controller(patron).Cancel(new QueuedTrackReferenceModel { Id = scheduled.Id })).Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Suggestions_ShouldHideTracksOutsideTheCupFilter()
    {
        var patron = AddPatron("patron");
        AddTrack("Bando Race");
        AddTrack("Bando Freestyle");

        var suggestions = (await Controller(patron).Suggestions(OpenClass, "bando")).Value!;

        suggestions.Select(s => s.Name).Should().Equal("Bando Race");
    }

    [Fact]
    public async Task Suggestions_ShouldExplainWhyATrackCannotBeScheduled()
    {
        var patron = AddPatron("patron");
        var queued = AddTrack("Rush queued");
        var badlyRated = AddTrack("Rush rated badly", rating: 2.4);
        var racedRecently = AddTrack("Rush raced recently");
        var racedLongAgo = AddTrack("Rush raced long ago", rating: 4);
        var racedOn = DateTime.Now.AddMonths(-2).Date;
        await QueueAsAdmin(WhoopClass, queued);
        AddRace(racedRecently, racedOn, cupId: WhoopClass);
        AddRace(racedLongAgo, DateTime.Now.AddMonths(-7));

        var suggestions = (await Controller(patron).Suggestions(OpenClass, "rush")).Value!;

        suggestions.ToDictionary(s => s.Name, s => s.Restriction).Should().BeEquivalentTo(new Dictionary<string, string?>
        {
            ["Rush queued"] = TrackSchedulingMessages.AlreadyScheduled,
            ["Rush rated badly"] = "Low rating (2.4)",
            ["Rush raced recently"] = $"Raced {racedOn:dd.MM.yyyy}, available from {racedOn.AddMonths(6):dd.MM.yyyy}",
            ["Rush raced long ago"] = null
        });
    }

    [Fact]
    public async Task Suggestions_ShouldNotCountACancelledCompetitionAsARace()
    {
        var patron = AddPatron("patron");
        var track = AddTrack("Swapped out");
        AddRace(track, DateTime.Now.AddDays(-1), CompetitionState.Cancelled);

        var suggestion = (await Controller(patron).Suggestions(OpenClass, "swapped")).Value!.Single();

        suggestion.Restriction.Should().BeNull();
    }

    [Fact]
    public async Task Schedule_ShouldRefuse_WhenTheTrackIsTodaysRunningCompetition()
    {
        var patron = AddPatron("patron");
        var track = AddTrack("Today's track");
        AddRace(track, DateTime.Now.AddHours(-3), CompetitionState.Started, cupId: WhoopClass);

        var result = await Controller(patron).Schedule(Request(track));

        ErrorOf(result).Should().StartWith("Raced " + DateTime.Now.AddHours(-3).ToString("dd.MM.yyyy"));
    }

    [Fact]
    public async Task Dequeue_ShouldTakeAdminAndPatronTracksInTheOrderTheyWereAdded_AfterATrackDatedToday()
    {
        var patron = AddPatron("patron");
        await QueueAsAdmin(OpenClass, AddTrack("Admin first"));
        await Controller(patron).Schedule(Request(AddTrack("Patron second")));
        await QueueAsAdmin(OpenClass, AddTrack("Admin third"));
        await QueueAsAdmin(OpenClass, AddTrack("Dated today"), DateTime.Today);

        var picked = new List<string>();
        for (var i = 0; i < 4; i++)
            picked.Add((await Queue().TryDequeueNextTrackAsync(OpenClass))!.Track.Name);

        picked.Should().Equal("Dated today", "Admin first", "Patron second", "Admin third");
    }

    [Fact]
    public async Task RandomPick_ShouldSkipTracksWaitingInTheQueue()
    {
        var queued = AddTrack("Queued");
        AddTrack("Free");
        await QueueAsAdmin(OpenClass, queued);
        var service = new TrackService(_db);
        var suitableTracks = await service.GetSuitableTracksAsync(new TrackFilter(new TrackFilterOptions()));

        for (var i = 0; i < 20; i++)
            (await service.GetRandomTrackAsync(suitableTracks)).Name.Should().Be("Free");
    }

    [Fact]
    public async Task Admin_ShouldSeeIssuesAsWarnings_AndStillScheduleTheTrack()
    {
        var track = AddTrack("Rough Freestyle", rating: 2);

        var suggestion = (await SchedulingService().SuggestAsync(OpenClass, "rough", SchedulingRules.Admin)).Single();
        var result = await SchedulingService().ScheduleAsync(new ScheduleTrackRequest(OpenClass, track, QuadId: null), SchedulingRules.Admin);

        suggestion.Issues.Select(issue => (issue.Message, issue.Blocking))
            .Should().Equal((TrackSchedulingMessages.OutsideCupFilter, false), ("Low rating (2.0)", false));
        result.Error.Should().BeNull();
    }

    [Theory]
    [InlineData("retired-cup", null, TrackSchedulingMessages.CupNotAvailable)]
    [InlineData(WhoopClass, TbsSpec, TrackSchedulingMessages.QuadNotAllowed)]
    public async Task Admin_ShouldFollowTheSameCupAndQuadRulesAsPatrons(string cupId, int? quadId, string error)
    {
        var result = await SchedulingService().ScheduleAsync(new ScheduleTrackRequest(cupId, AddTrack("Track"), quadId), SchedulingRules.Admin);

        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Admin_ShouldBeRefused_WhenTheDateIsTaken()
    {
        var date = DateTime.Today.AddDays(3);
        await QueueAsAdmin(OpenClass, AddTrack("First"), date);

        var result = await SchedulingService().ScheduleAsync(new ScheduleTrackRequest(OpenClass, AddTrack("Second"), QuadId: null, date), SchedulingRules.Admin);

        result.Error.Should().Be(TrackSchedulingMessages.DateTaken(date));
    }

    private TrackQueueService Queue() => new(_db);

    private TrackSchedulingService SchedulingService() =>
        new(_db, _cupService, Queue(), new PatronQuotaPolicy(_db, Options.Create(_options)));

    private async Task QueueAsAdmin(string cupId, string trackId, DateTime? scheduledOn = null)
    {
        var result = await SchedulingService().ScheduleAsync(new ScheduleTrackRequest(cupId, trackId, QuadId: null, scheduledOn), SchedulingRules.Admin);
        result.Error.Should().BeNull();
    }

    private TrackSchedulingController Controller(ApplicationUser? user)
    {
        var options = Options.Create(_options);
        var quotaPolicy = new PatronQuotaPolicy(_db, options);
        var service = new TrackSchedulingService(_db, _cupService, Queue(), quotaPolicy);

        return new TrackSchedulingController(_userManager, service, Queue(), quotaPolicy, _db, options)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(user is null
                        ? new ClaimsIdentity()
                        : new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"))
                }
            }
        };
    }

    private static ScheduleTrackModel Request(string trackId, int? quadId = null, string cupId = OpenClass) =>
        new() { CupId = cupId, TrackId = trackId, QuadId = quadId };

    private static string? ErrorOf(ActionResult<QueuedTrackModel> result) =>
        result.Result.Should().BeOfType<BadRequestObjectResult>().Subject.Value as string;

    private ApplicationUser AddUser(string name, bool withPilot = true)
    {
        var user = new ApplicationUser { Id = name, UserName = name, Email = $"{name}@test.local" };

        if (withPilot)
        {
            var pilot = new Pilot($"pilot_{name}") { Id = _nextPilotId++, Country = "UA" };
            _db.Set<Pilot>().Add(pilot);
            user.PilotId = pilot.Id;
        }

        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    private ApplicationUser AddPatron(string name, string tierName = "Level 2")
    {
        var user = AddUser(name);
        AddSupporter(user, tierName);
        return user;
    }

    private void AddSupporter(ApplicationUser user, string tierName, string status = PatreonSupporter.ActivePatronStatus, decimal amount = 5)
    {
        _db.Set<PatreonSupporter>().Add(new PatreonSupporter
        {
            PatreonId = user.Id, Name = user.Id, Status = status, Amount = amount, TierName = tierName, PilotId = user.PilotId
        });
        _db.SaveChanges();
    }

    private string AddTrack(string name, double? rating = null)
    {
        var track = new Track
        {
            TrackId = _nextTrackId++,
            Name = name,
            Scene = _scene,
            Rating = rating is null ? null : new TrackRating { Value = rating }
        };
        _db.Set<Track>().Add(track);
        _db.SaveChanges();
        return track.Id;
    }

    private void AddRace(string trackId, DateTime startedOn, CompetitionState state = CompetitionState.Closed, string cupId = OpenClass)
    {
        _db.Set<Competition>().Add(new Competition { CupId = cupId, TrackId = trackId, State = state, StartedOn = startedOn });
        _db.SaveChanges();
    }
}
