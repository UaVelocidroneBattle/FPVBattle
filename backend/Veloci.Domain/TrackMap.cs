namespace Veloci.Domain;

public class TrackMap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; }

    public int MapId { get; set; }

    public virtual IList<Track> Tracks { get; set; } = new List<Track>();
}
