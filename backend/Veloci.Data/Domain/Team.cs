namespace Veloci.Data.Domain;

public class Team
{
    public const int NameMaxLength = 32;
    public const int DescriptionMaxLength = 500;

    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public virtual ICollection<TeamMember> Members { get; set; } = [];
    public virtual ICollection<TeamInvite> Invites { get; set; } = [];
}
