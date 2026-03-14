using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Web.Controllers.TrackQueue;

public class TrackQueueController : Controller
{
    private readonly TrackQueueService _trackQueueService;
    private readonly ICupService _cupService;

    public TrackQueueController(TrackQueueService trackQueueService, ICupService cupService)
    {
        _trackQueueService = trackQueueService;
        _cupService = cupService;
    }

    public async Task<IActionResult> Index()
    {
        var cups = _cupService.GetAllCups();

        var cupModels = await Task.WhenAll(cups.Select(async kv => new CupQueueModel
        {
            CupId = kv.Key,
            CupName = kv.Value.Name,
            Tracks = await _trackQueueService.GetQueueAsync(kv.Key)
        }));

        return View(new TrackQueueViewModel { Cups = [..cupModels] });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string cupId, string trackName, DateTime? scheduleOn)
    {
        try
        {
            await _trackQueueService.QueueTrackAsync(cupId, trackName, scheduleOn);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid id)
    {
        try
        {
            await _trackQueueService.RemoveFromQueueAsync(id);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}