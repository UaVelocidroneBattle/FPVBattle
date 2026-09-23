using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.PilotBinding.Services;

namespace Veloci.Web.Controllers.Admin.Patrons;

public class PatronsController : AdminControllerBase
{
    private const string ActivePatronStatus = "active_patron";

    private readonly IRepository<PatreonSupporter> _supporters;
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<ApplicationUser> _users;
    private readonly PilotBindingService _bindingService;

    public PatronsController(
        IRepository<PatreonSupporter> supporters,
        IRepository<Pilot> pilots,
        IRepository<ApplicationUser> users,
        PilotBindingService bindingService)
    {
        _supporters = supporters;
        _pilots = pilots;
        _users = users;
        _bindingService = bindingService;
    }

    public async Task<IActionResult> Index()
    {
        var supporters = await _supporters.GetAll()
            .Include(s => s.Pilot)
            .OrderBy(s => s.Name)
            .ToListAsync();

        var linkedPilotIds = supporters
            .Where(s => s.PilotId is not null)
            .Select(s => s.PilotId!.Value)
            .ToList();

        var usersByEmail = await FindUsersMatchingUnlinkedPatronsAsync(supporters);

        var rows = supporters.Select(s =>
        {
            var matchedUser = s.PilotId is null && s.Email is not null
                ? usersByEmail.GetValueOrDefault(s.Email.ToUpperInvariant())
                : null;
            var suggestedPilot = matchedUser?.Pilot is { } pilot && !linkedPilotIds.Contains(pilot.Id) ? pilot : null;

            return new PatronRow
            {
                PatreonId = s.PatreonId,
                Name = s.Name,
                Email = s.Email,
                TierName = s.TierName,
                Amount = s.Amount,
                IsActive = s.Status == ActivePatronStatus,
                FirstSupportedAt = s.FirstSupportedAt,
                PilotName = s.Pilot?.Name,
                SuggestedPilotName = suggestedPilot?.Name,
                HasMatchingUserWithoutPilot = matchedUser is { PilotId: null }
            };
        }).ToList();

        var unlinkedPilotNames = await _pilots.GetAll()
            .Where(p => !linkedPilotIds.Contains(p.Id))
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToListAsync();

        return View(new PatronsViewModel { Patrons = rows, UnlinkedPilotNames = unlinkedPilotNames });
    }

    private async Task<Dictionary<string, ApplicationUser>> FindUsersMatchingUnlinkedPatronsAsync(
        IEnumerable<PatreonSupporter> supporters)
    {
        var normalizedEmails = supporters
            .Where(s => s.PilotId is null && s.Email is not null)
            .Select(s => s.Email!.ToUpperInvariant())
            .Distinct()
            .ToList();

        var users = await _users.GetAll()
            .Where(u => u.NormalizedEmail != null && normalizedEmails.Contains(u.NormalizedEmail))
            .Include(u => u.Pilot)
            .ToListAsync();

        return users.ToDictionary(u => u.NormalizedEmail!);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(string patreonId, string pilotName)
    {
        try
        {
            var supporter = await _supporters.FindAsync(patreonId)
                            ?? throw new InvalidOperationException("Patron not found");
            var pilot = await _bindingService.FindPilotByNameAsync(pilotName)
                        ?? throw new InvalidOperationException($"Pilot '{pilotName}' not found (names are case sensitive)");

            supporter.PilotId = pilot.Id;
            await _supporters.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink(string patreonId)
    {
        try
        {
            var supporter = await _supporters.FindAsync(patreonId)
                            ?? throw new InvalidOperationException("Patron not found");

            supporter.PilotId = null;
            await _supporters.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
