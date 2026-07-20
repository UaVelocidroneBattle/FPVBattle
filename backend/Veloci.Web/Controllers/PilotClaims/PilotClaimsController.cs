using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.PilotBinding.Services;

namespace Veloci.Web.Controllers.PilotClaims;

public class PilotClaimsController : AdminControllerBase
{
    private readonly IRepository<PilotClaim> _claims;
    private readonly IRepository<ApplicationUser> _users;
    private readonly IRepository<Pilot> _pilots;
    private readonly PilotBindingService _bindingService;

    public PilotClaimsController(
        IRepository<PilotClaim> claims,
        IRepository<ApplicationUser> users,
        IRepository<Pilot> pilots,
        PilotBindingService bindingService)
    {
        _claims = claims;
        _users = users;
        _pilots = pilots;
        _bindingService = bindingService;
    }

    public async Task<IActionResult> Index()
    {
        var claims = await _claims.GetAll().OrderBy(c => c.ExpiresOn).ToListAsync();

        var userIds = claims.Select(c => c.UserId).Distinct().ToList();
        var users = await _users.GetAll(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

        var claimedNames = claims.Select(c => c.PilotName).Distinct().ToList();
        var pilots = await _pilots.GetAll(p => claimedNames.Contains(p.Name)).ToListAsync();

        var pilotIds = pilots.Select(p => p.Id).ToList();
        var linkedPilotIds = await _users
            .GetAll(u => u.PilotId != null && pilotIds.Contains(u.PilotId.Value))
            .Select(u => u.PilotId!.Value)
            .ToListAsync();

        var rows = claims.Select(claim =>
        {
            var user = users.GetValueOrDefault(claim.UserId);
            var pilot = pilots.FirstOrDefault(p => string.Equals(p.Name, claim.PilotName, StringComparison.Ordinal));

            return new PilotClaimRow
            {
                ClaimId = claim.Id,
                UserEmail = user?.Email ?? "(deleted user)",
                UserName = user?.DisplayName,
                PilotName = claim.PilotName,
                CreatedOn = claim.CreatedOn,
                ExpiresOn = claim.ExpiresOn,
                IsExpired = claim.IsExpired,
                PilotExists = pilot is not null,
                PilotLinked = pilot is not null && linkedPilotIds.Contains(pilot.Id),
                UserLinked = user?.PilotId is not null
            };
        }).ToList();

        return View(new PilotClaimsViewModel { Claims = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int claimId)
    {
        try
        {
            var claim = await _claims.FindAsync(claimId)
                        ?? throw new InvalidOperationException("Claim not found");
            var user = await _users.FindAsync(claim.UserId)
                       ?? throw new InvalidOperationException("The claiming user no longer exists");
            var pilot = await _bindingService.FindPilotByNameAsync(claim.PilotName)
                        ?? throw new InvalidOperationException($"Pilot '{claim.PilotName}' has no results yet, there is nothing to link");

            // LinkAsync also removes the user's pending claim
            await _bindingService.LinkAsync(user, pilot);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int claimId)
    {
        try
        {
            var claim = await _claims.FindAsync(claimId)
                        ?? throw new InvalidOperationException("Claim not found");

            await _bindingService.DeleteClaimAsync(claim);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
