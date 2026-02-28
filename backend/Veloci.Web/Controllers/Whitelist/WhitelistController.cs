using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Services;

namespace Veloci.Web.Controllers.Whitelist;

public class WhitelistController : Controller
{
    private readonly IWhiteListService _whiteListService;

    public WhitelistController(IWhiteListService whiteListService)
    {
        _whiteListService = whiteListService;
    }

    public async Task<IActionResult> Index()
    {
        var pilots = await _whiteListService.GetWhitelistAsync();
        return View(pilots.OrderBy(p => p).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string pilotName)
    {
        try
        {
            await _whiteListService.AddToWhiteListAsync(pilotName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string pilotName)
    {
        try
        {
            await _whiteListService.RemoveFromWhiteListAsync(pilotName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
