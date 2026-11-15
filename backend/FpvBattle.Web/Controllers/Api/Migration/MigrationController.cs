using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace FpvBattle.Web.Controllers.Api.Migration;

[ApiController]
[Route("/api/migration/[action]")]
public class MigrationController : ControllerBase
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MigrationController>();
    private readonly IApplicationDbContext _db;

    public MigrationController(IApplicationDbContext db)
    {
        _db = db;
    }

    // Write a migration method here
}
