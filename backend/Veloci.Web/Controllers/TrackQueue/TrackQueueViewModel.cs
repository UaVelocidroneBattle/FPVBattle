using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.TrackQueue;

public class TrackQueueViewModel
{
    public List<CupQueueModel> Cups { get; set; } = [];
}

public class CupQueueModel
{
    public string CupId { get; set; } = string.Empty;
    public string CupName { get; set; } = string.Empty;
    public List<QueuedTrack> Tracks { get; set; } = [];
}