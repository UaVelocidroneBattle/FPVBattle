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

namespace Veloci.Tests;

/// <summary>In-memory SQLite database with users, pilots and the team service, shared by the team controller tests.</summary>
public abstract class TeamsTestBase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    protected readonly ApplicationDbContext _db;
    protected readonly TeamSettings _settings = new() { MaxTeamSize = 0, MinPublicTeamSize = 0, MaxOwnedTeamsPerUser = 1, TeamsPerUser = 1 };
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

    protected TeamService Service() => new(
        new Repository<Team>(_db), new Repository<TeamMember>(_db), new Repository<TeamInvite>(_db), Options.Create(_settings));
}
