using FluentAssertions;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FpvBattle.Tests;

/// <summary>
/// A track may wait in the queue only once from now on, so the migration removes duplicates admins queued
/// before. The earliest copy keeps its place, and raced entries stay as history.
/// </summary>
public class TrackSchedulingMigrationTests : IDisposable
{
    private const string MigrationBefore = "AddSceneLookups";
    private const string TrackSchedulingMigration = "AddTrackScheduling";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly IMigrator _migrator;

    public TrackSchedulingMigrationTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _migrator = _db.GetService<IMigrator>();

        _migrator.Migrate(MigrationBefore);

        Execute("""
            INSERT INTO Scenes (Id, Name, SceneId, IsDeleted) VALUES ('hangar', 'Hangar', 3, 0);
            INSERT INTO Tracks (Id, TrackId, Name, SceneId, IsDeleted) VALUES
                ('t1', 101, 'Twice queued', 'hangar', 0),
                ('t2', 102, 'Raced and queued', 'hangar', 0);
            INSERT INTO TrackQueue (Id, AddedOn, CupId, TrackId, Used) VALUES
                ('a', '2026-11-10 12:00:00', 'open-class', 't1', 0),
                ('b', '2026-11-11 12:00:00', 'whoop-class', 't1', 0),
                ('c', '2026-11-01 12:00:00', 'open-class', 't2', 1),
                ('d', '2026-11-12 12:00:00', 'open-class', 't2', 0);
            """);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Up_ShouldKeepTheEarliestWaitingCopyOfATrack_AndRacedEntries()
    {
        _migrator.Migrate(TrackSchedulingMigration);

        Query("SELECT Id FROM TrackQueue ORDER BY Id").Should().Equal("a", "c", "d");
    }

    [Fact]
    public void Up_ShouldRejectASecondWaitingCopyOfATrack_ButAllowItOnceRaced()
    {
        _migrator.Migrate(TrackSchedulingMigration);

        var waitingAgain = () => Execute("INSERT INTO TrackQueue (Id, AddedOn, CupId, TrackId, Used) VALUES ('e', '2026-11-13', 'open-class', 't1', 0)");
        var racedAgain = () => Execute("INSERT INTO TrackQueue (Id, AddedOn, CupId, TrackId, Used) VALUES ('f', '2026-11-13', 'open-class', 't1', 1)");

        waitingAgain.Should().Throw<SqliteException>();
        racedAgain.Should().NotThrow();
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
            values.Add(reader.GetValue(0).ToString()!);
        return values;
    }
}
