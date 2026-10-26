using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Veloci.Data;
using Veloci.Data.Domain;

namespace Veloci.Tests;

/// <summary>
/// The TeamInvites table is in production, so moving it to TeamMembershipRequests must keep every row.
/// </summary>
public class TeamMembershipRequestsMigrationTests : IDisposable
{
    private const string MigrationBefore = "AddPilotDayStreakFrozenUntil";
    private const string RenameMigration = "RenameTeamInvitesToMembershipRequests";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly IMigrator _migrator;

    public TeamMembershipRequestsMigrationTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _migrator = _db.GetService<IMigrator>();

        _migrator.Migrate(MigrationBefore);

        // Only the rows under test are seeded, so the users and teams they point to do not exist
        Execute("PRAGMA foreign_keys = OFF");
        Execute("""
            INSERT INTO TeamInvites (TeamId, UserId, InvitedByUserId, InvitedOn) VALUES
                (1, 'invitee', 'owner', '2026-10-01 10:00:00'),
                (2, 'asker', 'asker', '2026-10-02 11:30:00')
            """);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Up_KeepsEveryRowAndTurnsSelfInvitesIntoJoinRequests()
    {
        _migrator.Migrate(RenameMigration);

        var rows = _db.Set<TeamMembershipRequest>().AsNoTracking().OrderBy(r => r.UserId).ToList();

        rows.Should().HaveCount(2);
        rows[0].Should().BeEquivalentTo(new { TeamId = 2, UserId = "asker", InvitedByUserId = (string?)null, CreatedOn = new DateTime(2026, 10, 2, 11, 30, 0) });
        rows[1].Should().BeEquivalentTo(new { TeamId = 1, UserId = "invitee", InvitedByUserId = "owner", CreatedOn = new DateTime(2026, 10, 1, 10, 0, 0) });
    }

    [Fact]
    public void Down_RestoresTheOldTableWithItsRows()
    {
        _migrator.Migrate(RenameMigration);
        _migrator.Migrate(MigrationBefore);

        Query("SELECT UserId || ':' || InvitedByUserId || ':' || InvitedOn FROM TeamInvites ORDER BY UserId")
            .Should().Equal("asker:asker:2026-10-02 11:30:00", "invitee:owner:2026-10-01 10:00:00");
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
