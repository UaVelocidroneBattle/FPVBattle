using FpvBattle.Application.Features.Tracks.Queue;
using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Application.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace FpvBattle.Web.Controllers.Admin.TrackQueue;

public class TrackQueueController : AdminControllerBase
{
    private readonly TrackQueueService _queue;
    private readonly TrackSchedulingService _scheduling;
    private readonly IApplicationDbContext _db;

    public TrackQueueController(TrackQueueService queue, TrackSchedulingService scheduling, IApplicationDbContext db)
    {
        _queue = queue;
        _scheduling = scheduling;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var cups = new List<CupQueueModel>();

        foreach (var cup in await _scheduling.GetCupsAsync())
            cups.Add(new CupQueueModel { Cup = cup, Tracks = await _queue.GetQueueAsync(cup.CupId) });

        return View(new TrackQueueViewModel { Cups = cups });
    }

    [HttpGet]
    public async Task<IActionResult> SuggestTracks(string cupId, string term)
    {
        var suggestions = await _scheduling.SuggestAsync(cupId, term ?? string.Empty, SchedulingRules.Admin);
        var cupNames = (await _scheduling.GetCupsAsync()).ToDictionary(cup => cup.CupId, cup => cup.Name);

        return Json(suggestions.Select(suggestion => new ComboboxOption(
            Key: suggestion.Track.Id,
            Value: suggestion.Track.Name,
            Label: suggestion.Track.FullName,
            Notes: SuggestionNotes(suggestion, cupId, cupNames))));
    }

    // The issues admins may get past are shown as warnings, so they can decide
    private static List<ComboboxNote> SuggestionNotes(SchedulableTrack suggestion, string selectedCupId, Dictionary<string, string> cupNames)
    {
        var notes = suggestion.Issues.Select(issue => new ComboboxNote(issue.Message, Warning: true)).ToList();

        if (suggestion.Issues.Any(issue => issue.Kind == TrackIssueKind.RacedRecently))
            return notes;

        notes.Add(suggestion.LastRace is { } race
            ? new ComboboxNote(LastRaceText(race, selectedCupId, cupNames))
            : new ComboboxNote("never raced"));

        return notes;
    }

    private static string LastRaceText(LastRace race, string selectedCupId, Dictionary<string, string> cupNames)
    {
        var text = $"last raced {race.StartedOn:dd.MM.yyyy}";

        return race.CupId == selectedCupId
            ? text
            : $"{text} · {cupNames.GetValueOrDefault(race.CupId, race.CupId)}";
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string cupId, string? trackId, DateTime? scheduleOn, int? quadId)
    {
        if (string.IsNullOrEmpty(trackId))
        {
            TempData["Error"] = "Pick a track from the suggestions";
            return RedirectToAction(nameof(Index));
        }

        var result = await _scheduling.ScheduleAsync(new ScheduleTrackRequest(cupId, trackId, quadId, scheduleOn), SchedulingRules.Admin);

        if (result.Error is { } error)
            TempData["Error"] = error;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid id)
    {
        var entry = await _db.QueuedTracks.FindAsync(id);

        if (entry is null)
            TempData["Error"] = "This track is no longer in the queue";
        else if (await _scheduling.CancelAsync(entry) == CancelQueuedTrackResult.AlreadyRaced)
            TempData["Error"] = "This track has already been raced";

        return RedirectToAction(nameof(Index));
    }
}
