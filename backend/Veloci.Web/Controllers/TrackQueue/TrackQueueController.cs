using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services.Tracks;

namespace Veloci.Web.Controllers.TrackQueue;

public class TrackQueueController : Controller
{
    private readonly TrackQueueService _trackQueueService;
    private readonly ICupService _cupService;
    private readonly IRepository<QuadModel> _quads;

    public TrackQueueController(TrackQueueService trackQueueService, ICupService cupService, IRepository<QuadModel> quads)
    {
        _trackQueueService = trackQueueService;
        _cupService = cupService;
        _quads = quads;
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

        var quads = await _quads.GetAll().OrderBy(q => q.Name).ToListAsync();

        return View(new TrackQueueViewModel { Cups = [..cupModels], Quads = quads });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string cupId, string trackName, DateTime? scheduleOn, int? quadId)
    {
        try
        {
            await _trackQueueService.QueueTrackAsync(cupId, trackName, scheduleOn, quadId);
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