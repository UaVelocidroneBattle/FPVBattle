using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data;
using Veloci.Logic.Services;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController
{
    private readonly DbMigrator _dbMigrator;

    public MigrationController(
        DbMigrator dbMigrator)
    {
        _dbMigrator = dbMigrator;
    }

    [HttpGet("/api/migration/db")]
    public async Task DbMigration()
    {
        var sourceConnectionString = "DataSource=DB/whoop-app.db;Cache=Shared";

        var sourceOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(sourceConnectionString)
            .Options;

        await using var sourceDb = new ApplicationDbContext(sourceOptions);

        await _dbMigrator.MigrateAsync(sourceDb);
    }
}
