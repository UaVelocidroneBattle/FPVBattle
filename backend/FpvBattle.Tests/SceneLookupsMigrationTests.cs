using FluentAssertions;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FpvBattle.Tests;

/// <summary>
/// TrackMaps becomes Scenes in production, so the rename must keep every track, and competitions and the queue
/// must keep pointing at their tracks, even where duplicate scenes get merged.
/// </summary>
public class SceneLookupsMigrationTests : IDisposable
{
    private const string MigrationBefore = "AddTeamMembershipHistory";
    private const string SceneLookupsMigration = "AddSceneLookups";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;
    private readonly IMigrator _migrator;

    public SceneLookupsMigrationTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _migrator = _db.GetService<IMigrator>();

        _migrator.Migrate(MigrationBefore);

        Execute("""
            INSERT INTO TrackMaps (Id, Name, MapId) VALUES
                ('hangar', 'Hangar', 3),
                ('poly-small', 'Dynamic Polyworld', 44),
                ('poly-big', 'DynamicPoly', 44);
            INSERT INTO Tracks (Id, TrackId, Name, MapId) VALUES
                ('t1', 101, 'Hangar Race', 'hangar'),
                ('t2', 201, 'Poly Race 1', 'poly-small'),
                ('t3', 202, 'Poly Race 2', 'poly-big'),
                ('t4', 203, 'Poly Race 3', 'poly-big');
            INSERT INTO Competitions (Id, CupId, ResultsPosted, StartedOn, State, TrackId) VALUES
                ('c1', 'open-class', 1, '2026-11-01 00:03:00', 2, 't1'),
                ('c2', 'open-class', 1, '2026-11-02 00:03:00', 2, 't2');
            INSERT INTO TrackQueue (Id, AddedOn, CupId, TrackId, Used) VALUES
                ('8d6c3c39-5b1d-4f43-9a52-1f1b2c3d4e5f', '2026-11-10 12:00:00', 'open-class', 't3', 0);
            """);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Up_ShouldMergeDuplicateScenesIntoTheOneWithMostTracks()
    {
        _migrator.Migrate(SceneLookupsMigration);

        Query("SELECT Id || ':' || Name || ':' || SceneId || ':' || IsDeleted FROM Scenes ORDER BY SceneId")
            .Should().Equal("hangar:Hangar:3:0", "poly-big:DynamicPoly:44:0");
        Query("SELECT Id || ':' || SceneId || ':' || IsDeleted || ':' || IFNULL(Type, 'null') FROM Tracks ORDER BY Id")
            .Should().Equal("t1:hangar:0:null", "t2:poly-big:0:null", "t3:poly-big:0:null", "t4:poly-big:0:null");
    }

    [Fact]
    public void Up_ShouldKeepCompetitionsAndQueuedTracksOnTheirTracks()
    {
        _migrator.Migrate(SceneLookupsMigration);

        Query("SELECT Id || ':' || TrackId FROM Competitions ORDER BY Id").Should().Equal("c1:t1", "c2:t2");
        Query("SELECT TrackId FROM TrackQueue").Should().Equal("t3");
        Query("PRAGMA foreign_key_check").Should().BeEmpty();
    }

    [Fact]
    public void Up_ShouldRejectDuplicateVelocidroneIds()
    {
        _migrator.Migrate(SceneLookupsMigration);

        var duplicateScene = () => Execute("INSERT INTO Scenes (Id, Name, SceneId, IsDeleted) VALUES ('again', 'Hangar', 3, 0)");
        var duplicateTrack = () => Execute("INSERT INTO Tracks (Id, TrackId, Name, SceneId, IsDeleted) VALUES ('again', 101, 'Hangar Race', 'hangar', 0)");

        duplicateScene.Should().Throw<SqliteException>();
        duplicateTrack.Should().Throw<SqliteException>();
    }

    [Fact]
    public void Down_ShouldRestoreTrackMapsAndKeepEveryTrack()
    {
        _migrator.Migrate(SceneLookupsMigration);

        _migrator.Migrate(MigrationBefore);

        Query("SELECT Id || ':' || MapId FROM TrackMaps ORDER BY Id").Should().Equal("hangar:3", "poly-big:44");
        Query("SELECT Id || ':' || MapId FROM Tracks ORDER BY Id")
            .Should().Equal("t1:hangar", "t2:poly-big", "t3:poly-big", "t4:poly-big");
        Query("SELECT Id || ':' || TrackId FROM Competitions ORDER BY Id").Should().Equal("c1:t1", "c2:t2");
        Query("PRAGMA foreign_key_check").Should().BeEmpty();
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
