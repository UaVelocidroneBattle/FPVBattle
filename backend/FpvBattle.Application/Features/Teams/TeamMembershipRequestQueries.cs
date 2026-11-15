using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Teams;

public static class TeamMembershipRequestQueries
{
    public static IQueryable<TeamMembershipRequest> Invitations(this IQueryable<TeamMembershipRequest> query)
    {
        return query.Where(r => r.InvitedByUserId != null);
    }

    public static IQueryable<TeamMembershipRequest> JoinRequests(this IQueryable<TeamMembershipRequest> query)
    {
        return query.Where(r => r.InvitedByUserId == null);
    }
}
