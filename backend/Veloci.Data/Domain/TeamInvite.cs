namespace Veloci.Data.Domain;

/// <summary>
/// A pending link between a user and a team, either way round: an invitation from the team, or a join request
/// from the user, which is a row the user sent to themselves.
/// </summary>
public class TeamInvite
{
    public int TeamId { get; set; }
    public virtual Team Team { get; set; }
    public required string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }
    public required string InvitedByUserId { get; set; }
    public virtual ApplicationUser InvitedByUser { get; set; }
    public DateTime InvitedOn { get; set; } = DateTime.UtcNow;

    public bool IsJoinRequest => UserId == InvitedByUserId;
}

public static class TeamInviteExtensions
{
    public static IQueryable<TeamInvite> Invitations(this IQueryable<TeamInvite> query)
    {
        return query.Where(i => i.UserId != i.InvitedByUserId);
    }

    public static IQueryable<TeamInvite> JoinRequests(this IQueryable<TeamInvite> query)
    {
        return query.Where(i => i.UserId == i.InvitedByUserId);
    }
}
