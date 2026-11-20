using System.ComponentModel.DataAnnotations.Schema;

namespace FpvBattle.Domain;

public class Track : ISoftDeletable
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int TrackId { get; set; }

    public string Name { get; set; }

    public int? Type { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Scene Scene { get; set; }
    public string SceneId { get; set; }

    public virtual TrackRating? Rating { get; set; }

    [NotMapped]
    public string FullName => $"{Scene.Name} - {Name}";
}
