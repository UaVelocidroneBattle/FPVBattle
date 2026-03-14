namespace Veloci.Data.Domain;

public class QueuedTrack
{
    public Guid Id { get; set; }
    public string CupId { get; set; }
    public DateTime AddedOn { get; set; }
    public DateTime? ScheduledOn { get; set; }
    public virtual Track Track { get; set; }
    public string TrackId { get; set; }
    public bool Used { get; set; }
}
