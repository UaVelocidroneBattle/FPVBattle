using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.PilotBinding.Jobs;

/// <summary>
/// Removes pilot claims whose verification window has passed without a race,
/// so abandoned claims don't accumulate in the database.
/// </summary>
public class ExpiredClaimCleanupJob
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ExpiredClaimCleanupJob>();

    private readonly IRepository<PilotClaim> _claims;

    public ExpiredClaimCleanupJob(IRepository<PilotClaim> claims)
    {
        _claims = claims;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var deleted = await _claims.GetAll(c => c.ExpiresOn <= now).ExecuteDeleteAsync(ct);

        if (deleted > 0)
            Log.Information("Deleted {Count} expired pilot claims", deleted);
    }
}
