using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Teams;

/// <summary>
/// Who may do what inside a team. <see cref="Services.TeamService"/> only runs the operations, so callers acting
/// on behalf of a team member ask here first. Administrators acting from the admin area skip these checks.
/// </summary>
public static class TeamPermissions
{
    public static bool CanEditTeam(TeamMember? actor) => actor?.Role is TeamRole.Owner or TeamRole.Admin;

    public static bool CanInvite(TeamMember? actor) => CanEditTeam(actor);

    public static bool CanRevokeInvite(TeamMember? actor) => CanEditTeam(actor);

    public static bool CanDeleteTeam(TeamMember? actor) => actor?.Role is TeamRole.Owner;

    public static bool CanChangeRole(TeamMember? actor, TeamMember target) =>
        actor?.Role is TeamRole.Owner && actor.UserId != target.UserId;

    public static bool CanRemoveMember(TeamMember? actor, TeamMember target)
    {
        if (actor is null || actor.UserId == target.UserId)
            return false;

        return actor.Role is TeamRole.Owner || (actor.Role is TeamRole.Admin && target.Role is TeamRole.Member);
    }
}
