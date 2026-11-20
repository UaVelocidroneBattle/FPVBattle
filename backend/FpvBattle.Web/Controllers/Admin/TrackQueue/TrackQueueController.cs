using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Services.Tracks;
using FpvBattle.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Web.Controllers.Admin.TrackQueue;

public class TrackQueueController : AdminControllerBase
{
    private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<TrackQueueController>();

    private readonly TrackQueueService _trackQueueService;
    private readonly ICupService _cupService;
    private readonly IApplicationDbContext _db;

    public TrackQueueController(TrackQueueService trackQueueService, ICupService cupService, IApplicationDbContext db)
    {
        _trackQueueService = trackQueueService;
        _cupService = cupService;
        _db = db;
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

        var quads = await _db.QuadModels.OrderBy(q => q.Name).ToListAsync();

        return View(new TrackQueueViewModel { Cups = [..cupModels], Quads = quads });
    }

    [HttpGet]
    public async Task<IActionResult> SuggestTracks(string cupId, string term)
    {
        var suggestions = await _trackQueueService.SuggestTracksAsync(cupId, term ?? string.Empty);

        return Json(suggestions.Select(suggestion => new ComboboxOption(
            Key: suggestion.Track.Id,
            Value: suggestion.Track.Name,
            Label: suggestion.Track.FullName,
            Notes: SuggestionNotes(suggestion, cupId))));
    }

    private List<ComboboxNote> SuggestionNotes(TrackSuggestion suggestion, string selectedCupId)
    {
        var notes = new List<ComboboxNote>();

        if (!suggestion.MatchesCupFilter)
            notes.Add(new ComboboxNote("outside cup filter"));

        notes.Add(suggestion.LastRace is { } race
            ? new ComboboxNote(LastRaceText(race, selectedCupId), Warning: race.IsInReuseCooldown)
            : new ComboboxNote("never raced"));

        return notes;
    }

    private string LastRaceText(LastRace race, string selectedCupId)
    {
        var text = $"last raced {race.StartedOn:dd.MM.yyyy}";

        return race.CupId == selectedCupId
            ? text
            : $"{text} · {_cupService.GetCupOptions(race.CupId).Name}";
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string cupId, string? trackId, DateTime? scheduleOn, int? quadId)
    {
        if (string.IsNullOrEmpty(trackId))
        {
            TempData["Error"] = "Pick a track from the suggestions";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _trackQueueService.QueueTrackAsync(cupId, trackId, scheduleOn, quadId);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to queue track {TrackId} for cup {CupId}", trackId, cupId);
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
            Log.Warning(ex, "Failed to remove queued track {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}