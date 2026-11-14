using FluentAssertions;
using FpvBattle.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Tests.Architecture;

/// <summary>
/// Production data lives in a database built by the migrations, so the EF model must never drift from them.
/// Moving entities or configuration between projects is safe only while this test stays green.
/// </summary>
public class ModelSnapshotTests
{
    [Fact]
    public void Model_matches_the_latest_migration()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseLazyLoadingProxies()
            .UseSqlite(connection)
            .Options);

        db.Database.HasPendingModelChanges().Should().BeFalse(
            "every model change needs a migration; run 'dotnet ef migrations add' and check it preserves existing rows");
    }
}
