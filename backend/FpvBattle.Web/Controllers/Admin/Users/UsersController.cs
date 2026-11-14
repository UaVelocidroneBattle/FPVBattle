using FpvBattle.Application.Features.PilotBinding.Services;
using FpvBattle.Application.Helpers;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Admin.Users;

public class UsersController : AdminControllerBase
{
    private readonly IApplicationDbContext _db;
    private readonly PilotBindingService _bindingService;

    public UsersController(
        IApplicationDbContext db,
        PilotBindingService bindingService)
    {
        _db = db;
        _bindingService = bindingService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users
            .Include(u => u.Pilot)
            .ThenInclude(p => p.DayStreakFreezes)
            .OrderBy(u => u.Email)
            .ToListAsync();

        var rows = users.Select(u => new UserRow
        {
            UserId = u.Id,
            Email = u.Email ?? string.Empty,
            DisplayName = u.DisplayName,
            RegisteredOn = u.CreatedOn,
            PilotName = u.Pilot?.Name,
            CountryName = u.Pilot is null ? null : TextHelper.CountryName(u.Pilot.Country),
            DayStreak = u.Pilot?.DayStreak,
            Freezies = u.Pilot?.DayStreakFreezeCount,
            DayStreakFrozenUntil = u.Pilot?.DayStreakFrozenUntil
        }).ToList();

        return View(new UsersViewModel { Users = rows });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(string userId, string pilotName)
    {
        try
        {
            var user = await _db.Users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");
            var pilot = await _db.Pilots.FindByNameAsync(pilotName)
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
            var user = await _db.Users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            await _bindingService.UnlinkAsync(user);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFreezies(string userId, int amount)
    {
        try
        {
            var user = await _db.Users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");
            var pilot = user.Pilot
                        ?? throw new InvalidOperationException("User is not linked to a pilot");

            if (amount <= 0)
                throw new InvalidOperationException("Freezies amount must be greater than zero");

            var today = DateTime.Today;
            for (var i = 0; i < amount; i++)
            {
                pilot.DayStreakFreezes.Add(new DayStreakFreeze(today));
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> FreezeDayStreak(string userId, DateTime? until)
    {
        try
        {
            var user = await _db.Users.FindAsync(userId)
                       ?? throw new InvalidOperationException("User not found");
            var pilot = user.Pilot
                        ?? throw new InvalidOperationException("User is not linked to a pilot");

            pilot.DayStreakFrozenUntil = until?.Date;

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
