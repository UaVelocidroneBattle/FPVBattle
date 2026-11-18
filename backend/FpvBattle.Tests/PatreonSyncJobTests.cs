using FluentAssertions;
using FpvBattle.Application.Features.Patreon.Jobs;
using FpvBattle.Application.Features.Patreon.Models;
using FpvBattle.Application.Features.Patreon.Notifications;
using FpvBattle.Application.Features.Patreon.Services;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FpvBattle.Tests;

public class PatreonSyncJobTests : IDisposable
{
    private const string Active = PatreonSupporter.ActivePatronStatus;
    private const string Declined = "declined_patron";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly RecordingPublisher _publisher = new();
    private readonly FakePatreonService _patreon = new();
    private int _nextPilotId = 1;

    public PatreonSyncJobTests()
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
    public async Task new_paying_supporters_are_announced_together()
    {
        await Sync(Member("1", "kimi", Active, 1.2m), Member("2", "jack", Active, 5m), Member("3", "free", Active, 0m));

        AnnouncedNames().Should().Equal(["kimi", "jack"]);
    }

    [Fact]
    public async Task recovered_declined_payment_is_not_announced_again()
    {
        await Sync(Member("1", "kimi", Active, 1.2m));
        _publisher.Published.Clear();

        await Sync(Member("1", "kimi", Declined, 1.2m));
        await Sync(Member("1", "kimi", Active, 1.2m));

        _publisher.Published.Should().BeEmpty();
    }

    [Fact]
    public async Task new_supporter_is_linked_to_the_pilot_of_the_user_with_the_same_email()
    {
        var pilot = AddUserWithPilot("Kimi@Example.com");

        await Sync(Member("1", "kimi", Active, 1.2m, email: "kimi@example.com"));

        SupporterPilotId("1").Should().Be(pilot.Id);
    }

    [Fact]
    public async Task pilot_already_linked_to_another_supporter_is_not_linked_again()
    {
        var pilot = AddUserWithPilot("kimi@example.com");
        var oldAccount = Member("old", "kimi old account", Active, 0m);
        oldAccount.PilotId = pilot.Id;
        Supporters.Add(oldAccount);
        _db.SaveChanges();

        await Sync(Member("old", "kimi old account", Active, 0m), Member("1", "kimi", Active, 1.2m, email: "kimi@example.com"));

        SupporterPilotId("1").Should().BeNull();
    }

    [Fact]
    public async Task existing_supporter_unlinked_by_an_admin_stays_unlinked()
    {
        AddUserWithPilot("kimi@example.com");
        await Sync(Member("1", "kimi", Active, 1.2m, email: "kimi@example.com"));
        Supporters.Find("1")!.PilotId = null;
        _db.SaveChanges();

        await Sync(Member("1", "kimi", Active, 1.2m, email: "kimi@example.com"));

        SupporterPilotId("1").Should().BeNull();
    }

    private DbSet<PatreonSupporter> Supporters => _db.Set<PatreonSupporter>();

    private async Task Sync(params PatreonSupporter[] members)
    {
        _patreon.Members = members;
        var job = new PatreonSyncJob(
            _patreon,
            _db,
            new PatreonUserMatcher(_db),
            _publisher,
            Options.Create(new PatreonOptions { EnableSync = true }));

        await job.Handle(CancellationToken.None);
    }

    private IEnumerable<string> AnnouncedNames() => _publisher.Published
        .OfType<NewPatreonSupportersNotification>()
        .SelectMany(n => n.Supporters)
        .Select(s => s.Name);

    private int? SupporterPilotId(string patreonId) => Supporters
        .AsNoTracking()
        .Single(s => s.PatreonId == patreonId)
        .PilotId;

    private Pilot AddUserWithPilot(string email)
    {
        var pilot = new Pilot($"pilot_{_nextPilotId}") { Id = _nextPilotId++, Country = "UA" };
        _db.Set<Pilot>().Add(pilot);
        _db.Users.Add(new ApplicationUser
        {
            Id = email,
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            PilotId = pilot.Id
        });
        _db.SaveChanges();
        return pilot;
    }

    // The API returns fresh objects on every call, so each sync gets its own copy.
    private static PatreonSupporter Member(string id, string name, string status, decimal amount, string? email = null) => new()
    {
        PatreonId = id,
        Name = name,
        Email = email,
        Status = status,
        Amount = amount,
        FirstSupportedAt = new DateTime(2026, 6, 1),
        LastUpdated = DateTime.UtcNow
    };

    private class FakePatreonService : IPatreonService
    {
        public PatreonSupporter[] Members { get; set; } = [];

        public Task<PatreonCampaign[]> GetCampaignsAsync(CancellationToken ct) =>
            Task.FromResult(new[] { new PatreonCampaign { Id = "campaign", Type = "campaign" } });

        public Task<PatreonSupporter[]> GetCampaignMembersAsync(string campaignId, CancellationToken ct) =>
            Task.FromResult(Members.Select(m => Member(m.PatreonId, m.Name, m.Status, m.Amount ?? 0, m.Email)).ToArray());
    }
}
