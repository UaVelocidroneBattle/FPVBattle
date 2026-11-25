using System.Security.Claims;
using FluentAssertions;
using FpvBattle.Application.Bot.Discord;
using FpvBattle.Application.Bot.Telegram;
using FpvBattle.Application.Features.Auth;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Notifications;
using FpvBattle.Application.Services;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Auth;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Web.Controllers.Admin.Users;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Tests;

public class TrackRestartTests : IDisposable
{
    private const string Cup = "open-class";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RecordingPublisher _publisher = new();
    private readonly RecordingJobClient _jobs = new();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly CupService _cupService = new(Options.Create(new CupsConfiguration
    {
        Definitions =
        {
            [Cup] = new CupOptions { Name = "Open Class", IsEnabled = true },
            ["whoop-class"] = new CupOptions { Name = "Whoop Class", IsEnabled = true },
            ["retired-cup"] = new CupOptions { Name = "Retired", IsEnabled = false }
        }
    }));

    public TrackRestartTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();

        var normalizer = new UpperInvariantLookupNormalizer();
        _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(_db), null!, null!, null!, null!, normalizer, null!, null!, null!);
        _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(_db), [], normalizer, null!, null!);
    }

    public void Dispose()
    {
        _cache.Dispose();
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task RestartAsync_ShouldAnnounceTheRestartAndStartANewCompetition()
    {
        var restarted = await Service().RestartAsync(Cup, restartedBy: "kimtendo");

        restarted.Should().Be(TrackRestartResult.Restarted);
        _publisher.Published.Should().ContainSingle()
            .Which.Should().Be(new TrackRestart(Cup, "kimtendo"));
        _jobs.Jobs.Should().ContainSingle()
            .Which.Method.Name.Should().Be(nameof(CompetitionConductor.StartNewAsync));
    }

    [Fact]
    public async Task RestartAsync_ShouldDoNothing_WhenTheTrackWasJustRestarted()
    {
        await Service().RestartAsync(Cup, restartedBy: "kimtendo");
        _publisher.Published.Clear();

        var restarted = await Service().RestartAsync(Cup, restartedBy: "someone else");

        restarted.Should().Be(TrackRestartResult.RestartedMomentsAgo);
        _publisher.Published.Should().BeEmpty();
        _jobs.Jobs.Should().ContainSingle();
    }

    [Fact]
    public async Task RestartAsync_ShouldRestartAnotherCup_WhenOneCupWasJustRestarted()
    {
        await Service().RestartAsync(Cup, restartedBy: "kimtendo");

        var restarted = await Service().RestartAsync("whoop-class", restartedBy: "kimtendo");

        restarted.Should().Be(TrackRestartResult.Restarted);
        _jobs.Jobs.Should().HaveCount(2);
    }

    [Fact]
    public async Task RestartAsync_ShouldDoNothing_WhenTheCupIsDisabled()
    {
        var restarted = await Service().RestartAsync("retired-cup", restartedBy: "kimtendo");

        restarted.Should().Be(TrackRestartResult.CupNotEnabled);
        _publisher.Published.Should().BeEmpty();
        _jobs.Jobs.Should().BeEmpty();
    }

    [Fact]
    public async Task RestartAsync_ShouldStillStartANewCompetition_WhenTheAnnouncementFails()
    {
        var service = new TrackRestartService(new FailingPublisher(), _jobs, _cache, _cupService);

        var restart = () => service.RestartAsync(Cup, restartedBy: "kimtendo");

        await restart.Should().ThrowAsync<HttpRequestException>();
        _jobs.Jobs.Should().ContainSingle();
    }

    [Fact]
    public async Task CanRestartTrack_ShouldFollowTheConductorRole_WhenAnAdminAddsAndRemovesIt()
    {
        var user = AddUser("conductor");
        var controller = UsersController();

        (await CanRestartTrackAsync(user)).Should().BeFalse();

        await controller.AddRole(user.Id, Roles.Conductor);
        (await CanRestartTrackAsync(user)).Should().BeTrue();

        await controller.RemoveRole(user.Id, Roles.Conductor);
        (await CanRestartTrackAsync(user)).Should().BeFalse();
    }

    [Fact]
    public async Task AddRole_ShouldReportAnError_WhenTheRoleDoesNotExist()
    {
        var user = AddUser("pilot");
        var controller = UsersController();

        await controller.AddRole(user.Id, "Admin");

        controller.TempData["Error"].Should().Be("Role 'Admin' does not exist");
        (await _userManager.GetRolesAsync(user)).Should().BeEmpty();
    }

    [Fact]
    public void RestartMessages_ShouldNameThePilot_WhenAConductorRestartedTheTrack()
    {
        new TelegramMessageComposer().RestartTrack("kim*tendo").Should().Contain("Restarted by *kim\\*tendo*");
        new DiscordMessageComposer().RestartTrack("kimtendo").Should().Contain("Restarted by **kimtendo**");
    }

    [Fact]
    public void RestartMessages_ShouldNotNameAnyone_WhenNobodyIsGiven()
    {
        new TelegramMessageComposer().RestartTrack(null).Should().NotContain("Restarted by");
        new DiscordMessageComposer().RestartTrack(null).Should().NotContain("Restarted by");
    }

    private TrackRestartService Service() => new(_publisher, _jobs, _cache, _cupService);

    private UsersController UsersController()
    {
        var httpContext = new DefaultHttpContext();

        return new UsersController(_db, bindingService: null!, _userManager, _roleManager)
        {
            TempData = new TempDataDictionary(httpContext, new InMemoryTempDataProvider())
        };
    }

    private async Task<bool> CanRestartTrackAsync(ApplicationUser user)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"));
        var requirement = new StoredRoleRequirement(Roles.Conductor);
        var context = new AuthorizationHandlerContext([requirement], principal, resource: null);

        await new StoredRoleHandler(_userManager).HandleAsync(context);

        return context.HasSucceeded;
    }

    private ApplicationUser AddUser(string name)
    {
        var user = new ApplicationUser { Id = name, UserName = name, Email = $"{name}@test.local" };
        _db.Users.Add(user);
        _db.SaveChanges();
        return user;
    }

    private class RecordingJobClient : IBackgroundJobClient
    {
        public List<Job> Jobs { get; } = [];

        public string Create(Job job, IState state)
        {
            Jobs.Add(job);
            return Jobs.Count.ToString();
        }

        public bool ChangeState(string jobId, IState state, string expectedState) => true;
    }

    private class FailingPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            throw new HttpRequestException("Telegram is down");

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification =>
            throw new HttpRequestException("Telegram is down");
    }

    private class InMemoryTempDataProvider : ITempDataProvider
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context) => _values;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => _values = values;
    }
}
