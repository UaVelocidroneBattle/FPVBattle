using FpvBattle.Domain;

namespace FpvBattle.Web.Controllers.Admin.TrackQueue;

public class TrackQueueViewModel
{
    public List<CupQueueModel> Cups { get; set; } = [];
    public List<QuadModel> Quads { get; set; } = [];
}

public class CupQueueModel
{
    public string CupId { get; set; } = string.Empty;
    public string CupName { get; set; } = string.Empty;
    public List<QueuedTrack> Tracks { get; set; } = [];
}

public record ComboboxOption(string Key, string Value, string Label, IReadOnlyList<ComboboxNote> Notes);

public record ComboboxNote(string Text, bool Warning = false);
