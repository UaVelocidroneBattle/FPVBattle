namespace FpvBattle.Domain;

/// <summary>
/// A pending way into a team, from either side: an invitation from the team, sent by <see cref="InvitedByUser"/>,
/// or a join request from the user, which has no inviter.
/// </summary>
public class TeamMembershipRequest
{
    public int TeamId { get; set; }
    public virtual Team Team { get; set; }
    public required string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public string? InvitedByUserId { get; set; }
    public virtual ApplicationUser? InvitedByUser { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public bool IsJoinRequest => InvitedByUserId is null;
}
