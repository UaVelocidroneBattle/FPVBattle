using FluentAssertions;
using FpvBattle.Application.Features.Teams;
using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FpvBattle.Tests;

/// <summary>
/// TeamMembers is in production, so giving it a surrogate key must keep every membership.
/// </summary>
public class TeamMembershipHistoryMigrationTests : IDisposable
{
    private const string MigrationBefore = "AddPatreonSupporterFirstPaidAt";
    private const string HistoryMigration = "AddTeamMembershipHistory";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly IMigrator _migrator;

    public TeamMembershipHistoryMigrationTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _migrator = _db.GetService<IMigrator>();

        _migrator.Migrate(MigrationBefore);

        Execute("""
            INSERT INTO AspNetUsers (Id, AccessFailedCount, EmailConfirmed, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled, CreatedOn) VALUES
                ('owner', 0, 0, 0, 0, 0, '2026-10-01 09:00:00'),
                ('member', 0, 0, 0, 0, 0, '2026-10-01 09:00:00');
            INSERT INTO Teams (Id, Name, CreatedOn, IsDeleted) VALUES (1, 'Team', '2026-10-01 10:00:00', 0);
            INSERT INTO TeamMembers (TeamId, UserId, Role, JoinedOn) VALUES
                (1, 'owner', 2, '2026-10-01 10:00:00'),
                (1, 'member', 0, '2026-10-02 11:30:00');
            """);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Up_KeepsEveryMembershipAsCurrent()
    {
        _migrator.Migrate(HistoryMigration);

        var rows = _db.Set<TeamMember>().AsNoTracking().IgnoreQueryFilters().OrderBy(m => m.JoinedOn).ToList();

        rows.Select(m => m.Id).Should().OnlyHaveUniqueItems().And.NotContain(0);
        rows.Should().BeEquivalentTo(new[]
        {
            new { TeamId = 1, UserId = "owner", Role = TeamRole.Owner, JoinedOn = new DateTime(2026, 10, 1, 10, 0, 0), LeftOn = (DateTime?)null },
            new { TeamId = 1, UserId = "member", Role = TeamRole.Member, JoinedOn = new DateTime(2026, 10, 2, 11, 30, 0), LeftOn = (DateTime?)null }
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Up_AllowsAnEndedMembershipNextToTheCurrentOne()
    {
        _migrator.Migrate(HistoryMigration);

        Execute("UPDATE TeamMembers SET LeftOn = '2026-10-05 12:00:00' WHERE UserId = 'member'");
        Execute("INSERT INTO TeamMembers (TeamId, UserId, Role, JoinedOn) VALUES (1, 'member', 0, '2026-10-06 12:00:00')");

        _db.Set<TeamMember>().AsNoTracking().IncludingFormer().Count(m => m.UserId == "member").Should().Be(2);
    }

    [Fact]
    public void Down_KeepsCurrentMembershipsAndDropsTheHistory()
    {
        _migrator.Migrate(HistoryMigration);
        Execute("UPDATE TeamMembers SET LeftOn = '2026-10-05 12:00:00' WHERE UserId = 'member'");
        Execute("INSERT INTO TeamMembers (TeamId, UserId, Role, JoinedOn) VALUES (1, 'member', 1, '2026-10-06 12:00:00')");

        _migrator.Migrate(MigrationBefore);

        Query("SELECT UserId || ':' || Role FROM TeamMembers ORDER BY UserId").Should().Equal("member:1", "owner:2");
    }

    private void Execute(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private List<string> Query(string sql)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();

        var values = new List<string>();
        while (reader.Read())
            values.Add(reader.GetString(0));
        return values;
    }
}
