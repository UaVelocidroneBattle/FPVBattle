using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams;
using Veloci.Logic.Features.Teams.Services;
using Veloci.Web.Controllers.Api.Teams;

namespace Veloci.Tests;

/// <summary>In-memory SQLite database with users, pilots and the team service, shared by the team controller tests.</summary>
public abstract class TeamsTestBase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    protected readonly ApplicationDbContext _db;
    protected readonly TeamSettings _settings = new() { MaxTeamSize = 0, MinPublicTeamSize = 0, MinRaceDaysToCreate = 0 };
    protected readonly UserManager<ApplicationUser> _userManager;
    private int _nextPilotId = 1;
    private int _nextTag = 1;

    protected TeamsTestBase()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(_connection)
            .Options);
        _db.Database.EnsureCreated();

        _userManager = new UserManager<ApplicationUser>(
            new UserStore<ApplicationUser>(_db), null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    protected ApplicationUser AddUser(string name, bool withPilot = true)
    {
        var user = new ApplicationUser { Id = name, UserName = name, Email = $"{name}@test.local", DisplayName = $"Real Name of {name}" };

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

    protected string PilotNameOf(ApplicationUser user) => _db.Set<Pilot>().Find(user.PilotId)!.Name;

    /// <summary>A tag no other team in this test has used yet.</summary>
    protected string NextTag() => $"T{_nextTag++}";

    protected readonly RecordingPublisher _publisher = new();

    protected TeamService Service() => new(
        new Repository<Team>(_db), new Repository<TeamMember>(_db), new Repository<TeamInvite>(_db), Options.Create(_settings), _publisher);

    /// <summary>Keeps the published events instead of running their handlers.</summary>
    protected class RecordingPublisher : IPublisher
    {
        public List<object> Published { get; } = [];

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            Published.Add(notification);
            return Task.CompletedTask;
        }
    }

    protected static TeamMemberReferenceModel MemberRef(int teamId, ApplicationUser user) =>
        new() { TeamId = teamId, UserId = user.Id };

    protected static SetMemberRoleModel RoleChange(int teamId, ApplicationUser user, TeamRole role) =>
        new() { TeamId = teamId, UserId = user.Id, Role = role };

    /// <summary>A controller acting as the given user, or as a visitor when there is none.</summary>
    protected TeamsController Controller(ApplicationUser? user)
    {
        var controller = new TeamsController(
            _userManager,
            Service(),
            new TeamCreationPolicy(
                new Repository<Pilot>(_db), new Repository<TeamMember>(_db), new Repository<PatreonSupporter>(_db), Options.Create(_settings)),
            new Repository<Team>(_db),
            new Repository<TeamMember>(_db),
            new Repository<TeamInvite>(_db),
            new Repository<Pilot>(_db),
            Options.Create(_settings));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(user is null
                    ? new ClaimsIdentity()
                    : new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"))
            }
        };

        return controller;
    }
}
