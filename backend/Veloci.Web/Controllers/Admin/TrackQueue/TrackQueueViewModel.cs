using Veloci.Data.Domain;

namespace Veloci.Web.Controllers.Admin.TrackQueue;

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