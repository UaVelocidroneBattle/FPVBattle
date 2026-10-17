namespace Veloci.Data.Domain;

public class TeamInvite
{
    public int TeamId { get; set; }
    public virtual Team Team { get; set; }
    public required string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public required string InvitedByUserId { get; set; }
    public virtual ApplicationUser InvitedByUser { get; set; }
    public DateTime InvitedOn { get; set; } = DateTime.UtcNow;
}
