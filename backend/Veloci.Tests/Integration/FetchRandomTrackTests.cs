using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Veloci.Data;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Tests.Integration;

public class FetchRandomTrackTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly ApplicationDbContext _db;

    public FetchRandomTrackTests()
    {
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    [Trait ("Category", "Integration")]
    public async Task can_get_random_track()
    {
        var ts = new TrackService(
            new WebTrackFetcher(),
            new Repository<Track>(_db),
            new Repository<TrackMap>(_db),
            new Repository<Competition>(_db));

        var track = await ts.GetRandomTrackAsync(new TrackFilter(new TrackFilterOptions()));

        track.Name.Should().NotBeNullOrWhiteSpace();
        track.TrackId.Should().BeGreaterThan(0);
    }
}
