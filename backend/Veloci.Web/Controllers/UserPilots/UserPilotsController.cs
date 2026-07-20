using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.PilotBinding.Services;

namespace Veloci.Web.Controllers.UserPilots;

public class UserPilotsController : AdminControllerBase
{
    private readonly IRepository<ApplicationUser> _users;
    private readonly PilotBindingService _bindingService;

    public UserPilotsController(IRepository<ApplicationUser> users, PilotBindingService bindingService)
    {
        _users = users;
        _bindingService = bindingService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _users.GetAll()
            .Include(u => u.Pilot)
            .OrderBy(u => u.Email)
            .ToListAsync();

        var rows = users.Select(u => new UserPilotRow
        {
            UserId = u.Id,
            Email = u.Email ?? string.Empty,
            DisplayName = u.DisplayName,
            PilotName = u.Pilot?.Name
        }).ToList();

        return View(new UserPilotsViewModel { Users = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(string userId, string pilotName)
    {
        try
        {
            var user = await _users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");
            var pilot = await _bindingService.FindPilotByNameAsync(pilotName)
                        ?? throw new InvalidOperationException($"Pilot '{pilotName}' not found (names are case sensitive)");

            await _bindingService.LinkAsync(user, pilot);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink(string userId)
    {
        try
        {
            var user = await _users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            await _bindingService.UnlinkAsync(user);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
