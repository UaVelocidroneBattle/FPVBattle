using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Infrastructure.Persistence.Queries;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Logic.Features.PilotBinding.Services;

public enum ClaimPilotResult
{
    Claimed,
    AlreadyLinked,
    PilotTaken,
    PilotClaimPending
}

public class PilotBindingService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PilotBindingService>();

    /// <summary>How long a claim stays open waiting for the verification race.</summary>
    public static readonly TimeSpan ClaimLifetime = TimeSpan.FromHours(48);

    private readonly IApplicationDbContext _db;

    public PilotBindingService(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsPilotLinkedAsync(int pilotId)
    {
        return await _db.Users.AnyAsync(u => u.PilotId == pilotId);
    }

    public async Task<PilotClaim?> GetClaimAsync(ApplicationUser user)
    {
        return await _db.PilotClaims.FirstOrDefaultAsync(c => c.UserId == user.Id);
    }

    public async Task<ClaimPilotResult> ClaimAsync(ApplicationUser user, string pilotName)
    {
        if (user.PilotId is not null)
            return ClaimPilotResult.AlreadyLinked;

        var normalizedName = pilotName.Trim();
        var pilot = await _db.Pilots.FindByNameAsync(normalizedName);

        if (pilot is not null && await IsPilotLinkedAsync(pilot.Id))
            return ClaimPilotResult.PilotTaken;

        if (await IsPilotClaimedByAnotherUserAsync(normalizedName, user))
            return ClaimPilotResult.PilotClaimPending;

        var claim = await GetClaimAsync(user);
        var isNew = claim is null;

        claim ??= new PilotClaim { UserId = user.Id };
        claim.PilotName = normalizedName;
        claim.CreatedOn = DateTime.UtcNow;
        claim.ExpiresOn = claim.CreatedOn + ClaimLifetime;

        if (isNew)
            _db.PilotClaims.Add(claim);

        await _db.SaveChangesAsync();

        Log.Information("User {UserId} claimed pilot {PilotName}, awaiting verification race until {ExpiresOn}",
            user.Id, claim.PilotName, claim.ExpiresOn);

        return ClaimPilotResult.Claimed;
    }

    private async Task<bool> IsPilotClaimedByAnotherUserAsync(string pilotName, ApplicationUser user)
    {
        // Pilot names are case sensitive, hence the in-memory ordinal check on top
        // of the collation-dependent database filter.
        var now = DateTime.UtcNow;
        var candidateNames = await _db.PilotClaims.Where(c => c.UserId != user.Id && c.PilotName == pilotName && c.ExpiresOn > now)
            .Select(c => c.PilotName)
            .ToListAsync();

        return candidateNames.Any(n => string.Equals(n, pilotName, StringComparison.Ordinal));
    }

    public async Task CancelClaimAsync(ApplicationUser user)
    {
        var claim = await GetClaimAsync(user);

        if (claim is null)
            return;

        await RemoveClaimAsync(claim.Id);
        await _db.SaveChangesAsync();
        Log.Information("User {UserId} cancelled the claim for pilot {PilotName}", user.Id, claim.PilotName);
    }

    /// <summary>
    /// Links a user to a pilot directly, bypassing fly-to-verify. Admin action.
    /// Removes the user's pending claim, if any, since it is no longer relevant.
    /// </summary>
    public async Task LinkAsync(ApplicationUser user, Pilot pilot)
    {
        if (user.PilotId is not null)
            throw new InvalidOperationException($"User {user.Email} is already linked to a pilot");

        if (await IsPilotLinkedAsync(pilot.Id))
            throw new InvalidOperationException($"Pilot {pilot.Name} is already linked to another user");

        user.PilotId = pilot.Id;
        await _db.SaveChangesAsync();

        var claim = await GetClaimAsync(user);

        if (claim is not null)
        {
            await RemoveClaimAsync(claim.Id);
            await _db.SaveChangesAsync();
        }

        Log.Information("Manually linked user {UserId} to pilot {PilotName} ({PilotId})", user.Id, pilot.Name, pilot.Id);
    }

    /// <summary>
    /// Removes the link between a user and their pilot. Admin action.
    /// </summary>
    public async Task UnlinkAsync(ApplicationUser user)
    {
        if (user.PilotId is null)
            return;

        var pilotId = user.PilotId;
        user.PilotId = null;
        await _db.SaveChangesAsync();

        Log.Information("Unlinked user {UserId} from pilot {PilotId}", user.Id, pilotId);
    }

    /// <summary>
    /// Deletes a pending claim without completing it. Admin action.
    /// </summary>
    public async Task DeleteClaimAsync(PilotClaim claim)
    {
        await RemoveClaimAsync(claim.Id);
        await _db.SaveChangesAsync();
        Log.Information("Deleted claim of user {UserId} for pilot {PilotName}", claim.UserId, claim.PilotName);
    }

    /// <summary>
    /// Completes active claims for pilots that just posted a result.
    /// Called from the results pipeline with the pilots of the current update.
    /// </summary>
    public async Task CompleteClaimsAsync(IReadOnlyCollection<Pilot> racedPilots, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var activeClaims = await _db.PilotClaims.Where(c => c.ExpiresOn > now).ToListAsync(ct);

        foreach (var claim in activeClaims)
        {
            var pilot = racedPilots.FirstOrDefault(p => p.Name == claim.PilotName);

            if (pilot is null)
                continue;

            await CompleteClaimAsync(claim, pilot, ct);
        }
    }

    private async Task CompleteClaimAsync(PilotClaim claim, Pilot pilot, CancellationToken ct)
    {
        if (await IsPilotLinkedAsync(pilot.Id))
        {
            await RemoveClaimAsync(claim.Id);
            await _db.SaveChangesAsync(ct);
            Log.Warning("Removed claim of user {UserId} for pilot {PilotName}: the pilot is already linked to another account",
                claim.UserId, pilot.Name);
            return;
        }

        var user = await _db.Users.FindAsync(claim.UserId);

        if (user is null || user.PilotId is not null)
        {
            await RemoveClaimAsync(claim.Id);
            await _db.SaveChangesAsync(ct);
            return;
        }

        user.PilotId = pilot.Id;
        await RemoveClaimAsync(claim.Id);
        await _db.SaveChangesAsync(ct);

        Log.Information("Linked user {UserId} to pilot {PilotName} ({PilotId}) after a verified race",
            user.Id, pilot.Name, pilot.Id);
    }

    private async Task RemoveClaimAsync(int claimId)
    {
        var claim = await _db.PilotClaims.FindAsync(claimId)
            ?? throw new NullReferenceException($"Record with id: {claimId} does not exist");

        _db.PilotClaims.Remove(claim);
    }
}
