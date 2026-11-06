namespace Veloci.Data.Domain;

public class Team
{
    public const int NameMaxLength = 32;
    public const int DescriptionMaxLength = 500;
    public const int TagMinLength = 2;
    public const int TagMaxLength = 5;

    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Tag { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public virtual ICollection<TeamMember> Members { get; set; } = [];
    public virtual ICollection<TeamMembershipRequest> MembershipRequests { get; set; } = [];

    /// <summary>
    /// The team stays in the database so its past competition results keep their name and tag.
    /// Its members and pending requests go, which frees them to join another team.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        Members.Clear();
        MembershipRequests.Clear();
    }
}
