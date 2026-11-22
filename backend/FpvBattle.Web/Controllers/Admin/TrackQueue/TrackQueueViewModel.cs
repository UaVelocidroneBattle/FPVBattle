using FpvBattle.Application.Features.Tracks.Scheduling;
using FpvBattle.Domain;

namespace FpvBattle.Web.Controllers.Admin.TrackQueue;

public class TrackQueueViewModel
{
    public List<CupQueueModel> Cups { get; set; } = [];
}

public class CupQueueModel
{
    public required SchedulingCup Cup { get; set; }
    public List<QueuedTrack> Tracks { get; set; } = [];
}

public record ComboboxOption(string Key, string Value, string Label, IReadOnlyList<ComboboxNote> Notes);

public record ComboboxNote(string Text, bool Warning = false);
