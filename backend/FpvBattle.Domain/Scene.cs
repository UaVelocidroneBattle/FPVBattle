namespace FpvBattle.Domain;

public class Scene : ISoftDeletable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; }

    public int SceneId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual IList<Track> Tracks { get; set; } = new List<Track>();
}
