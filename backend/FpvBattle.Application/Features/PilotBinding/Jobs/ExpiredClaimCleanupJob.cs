using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Features.PilotBinding.Jobs;

/// <summary>
/// Removes pilot claims whose verification window has passed without a race,
/// so abandoned claims don't accumulate in the database.
/// </summary>
public class ExpiredClaimCleanupJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ExpiredClaimCleanupJob>();

    private readonly IApplicationDbContext _db;

    public ExpiredClaimCleanupJob(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var deleted = await _db.PilotClaims.Where(c => c.ExpiresOn <= now).ExecuteDeleteAsync(ct);

        if (deleted > 0)
            Log.Information("Deleted {Count} expired pilot claims", deleted);
    }
}
