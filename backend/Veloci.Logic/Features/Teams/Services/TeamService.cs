using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Teams.Services;

public class TeamService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TeamService>();

    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<TeamInvite> _invites;
    private readonly TeamSettings _settings;

    public TeamService(
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<TeamInvite> invites,
        IOptions<TeamSettings> settings)
    {
        _teams = teams;
        _members = members;
        _invites = invites;
        _settings = settings.Value;
    }

    public async Task<CreateTeamResult> CreateAsync(ApplicationUser owner, string name, string? description)
    {
        if (owner.PilotId is null)
            return new CreateTeamResult(CreateTeamStatus.PilotNotLinked);

        var (normalizedName, normalizedDescription) = Normalize(name, description);

        if (normalizedName.Length is 0 or > Team.NameMaxLength)
            return new CreateTeamResult(CreateTeamStatus.InvalidName);

        if (normalizedDescription?.Length > Team.DescriptionMaxLength)
            return new CreateTeamResult(CreateTeamStatus.DescriptionTooLong);

        if (await HasReachedMembershipLimitAsync(owner))
            return new CreateTeamResult(CreateTeamStatus.MembershipLimitReached);

        if (await CountOwnedTeamsAsync(owner) >= _settings.MaxOwnedTeamsPerUser)
            return new CreateTeamResult(CreateTeamStatus.OwnedTeamsLimitReached);

        if (await _teams.GetAll().AnyAsync(t => t.Name == normalizedName))
            return new CreateTeamResult(CreateTeamStatus.NameTaken);

        var team = new Team
        {
            Name = normalizedName,
            Description = normalizedDescription
        };

        team.Members.Add(new TeamMember
        {
            UserId = owner.Id,
            Role = TeamRole.Owner
        });

        await _teams.AddAsync(team);
        await _teams.SaveChangesAsync();

        Log.Information("User {UserId} created team {TeamName} ({TeamId})", owner.Id, team.Name, team.Id);

        return new CreateTeamResult(CreateTeamStatus.Created, team);
    }

    public async Task<UpdateTeamResult> UpdateAsync(ApplicationUser actor, Team team, string name, string? description)
    {
        var actorRole = (await FindMembershipAsync(team.Id, actor.Id))?.Role;

        if (actorRole is not (TeamRole.Owner or TeamRole.Admin))
            return UpdateTeamResult.NotAllowed;

        var (normalizedName, normalizedDescription) = Normalize(name, description);

        if (normalizedName.Length is 0 or > Team.NameMaxLength)
            return UpdateTeamResult.InvalidName;

        if (normalizedDescription?.Length > Team.DescriptionMaxLength)
            return UpdateTeamResult.DescriptionTooLong;

        if (await _teams.GetAll().AnyAsync(t => t.Id != team.Id && t.Name == normalizedName))
            return UpdateTeamResult.NameTaken;

        team.Name = normalizedName;
        team.Description = normalizedDescription;
        await _teams.SaveChangesAsync();

        Log.Information("User {UserId} updated team {TeamName} ({TeamId})", actor.Id, team.Name, team.Id);

        return UpdateTeamResult.Updated;
    }

    public async Task<RemoveTeamResult> RemoveAsync(ApplicationUser actor, Team team)
    {
        var isOwner = await _members.GetAll().AnyAsync(m =>
            m.TeamId == team.Id && m.UserId == actor.Id && m.Role == TeamRole.Owner);

        if (!isOwner)
            return RemoveTeamResult.NotOwner;

        await _teams.RemoveAsync(team.Id);
        await _teams.SaveChangesAsync();

        Log.Information("User {UserId} removed team {TeamName} ({TeamId})", actor.Id, team.Name, team.Id);

        return RemoveTeamResult.Removed;
    }

    public async Task<InviteToTeamResult> InviteAsync(ApplicationUser inviter, Team team, ApplicationUser invitee)
    {
        var teamMembers = _members.GetAll().Where(m => m.TeamId == team.Id);

        var inviterRole = await teamMembers
            .Where(m => m.UserId == inviter.Id)
            .Select(m => (TeamRole?)m.Role)
            .FirstOrDefaultAsync();

        if (inviterRole is not (TeamRole.Owner or TeamRole.Admin))
            return InviteToTeamResult.NotAllowed;

        if (await teamMembers.AnyAsync(m => m.UserId == invitee.Id))
            return InviteToTeamResult.AlreadyMember;

        if (await _invites.GetAll().AnyAsync(i => i.TeamId == team.Id && i.UserId == invitee.Id))
            return InviteToTeamResult.AlreadyInvited;

        if (_settings.MaxTeamSize > 0 && await teamMembers.CountAsync() >= _settings.MaxTeamSize)
            return InviteToTeamResult.TeamFull;

        if (await HasReachedMembershipLimitAsync(invitee))
            return InviteToTeamResult.InviteeMembershipLimitReached;

        await _invites.AddAsync(new TeamInvite
        {
            TeamId = team.Id,
            UserId = invitee.Id,
            InvitedByUserId = inviter.Id
        });

        await _invites.SaveChangesAsync();

        Log.Information("User {InviterId} invited user {InviteeId} to team {TeamName} ({TeamId})",
            inviter.Id, invitee.Id, team.Name, team.Id);

        return InviteToTeamResult.Invited;
    }

    public async Task<AcceptInviteResult> AcceptInviteAsync(ApplicationUser user, TeamInvite invite)
    {
        if (invite.UserId != user.Id)
            return AcceptInviteResult.NotInvitee;

        var memberCount = await _members.GetAll().CountAsync(m => m.TeamId == invite.TeamId);

        if (_settings.MaxTeamSize > 0 && memberCount >= _settings.MaxTeamSize)
            return AcceptInviteResult.TeamFull;

        // An invite sent earlier stays valid after the user joins another team, so the limit is checked again here.
        if (await HasReachedMembershipLimitAsync(user))
            return AcceptInviteResult.MembershipLimitReached;

        await _members.AddAsync(new TeamMember
        {
            TeamId = invite.TeamId,
            UserId = user.Id,
            Role = TeamRole.Member
        });

        await _invites.RemoveEntryAsync(invite);
        await _members.SaveChangesAsync();

        Log.Information("User {UserId} accepted the invite to team {TeamId}", user.Id, invite.TeamId);

        return AcceptInviteResult.Accepted;
    }

    public async Task<DeclineInviteResult> DeclineInviteAsync(ApplicationUser user, TeamInvite invite)
    {
        if (invite.UserId != user.Id)
            return DeclineInviteResult.NotInvitee;

        await _invites.RemoveEntryAsync(invite);
        await _invites.SaveChangesAsync();

        Log.Information("User {UserId} declined the invite to team {TeamId}", user.Id, invite.TeamId);

        return DeclineInviteResult.Declined;
    }

    public async Task<LeaveTeamResult> LeaveAsync(ApplicationUser user, Team team)
    {
        var membership = await FindMembershipAsync(team.Id, user.Id);

        if (membership is null)
            return LeaveTeamResult.NotMember;

        if (membership.Role is TeamRole.Owner)
            return LeaveTeamResult.OwnerCannotLeave;

        await _members.RemoveEntryAsync(membership);
        await _members.SaveChangesAsync();

        Log.Information("User {UserId} left team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        return LeaveTeamResult.Left;
    }

    public async Task<RevokeInviteResult> RevokeInviteAsync(ApplicationUser actor, TeamInvite invite)
    {
        var actorRole = (await FindMembershipAsync(invite.TeamId, actor.Id))?.Role;

        if (actorRole is not (TeamRole.Owner or TeamRole.Admin))
            return RevokeInviteResult.NotAllowed;

        await _invites.RemoveEntryAsync(invite);
        await _invites.SaveChangesAsync();

        Log.Information("User {UserId} revoked the invite of user {InviteeId} to team {TeamId}",
            actor.Id, invite.UserId, invite.TeamId);

        return RevokeInviteResult.Revoked;
    }

    public async Task<SetMemberRoleResult> SetRoleAsync(ApplicationUser actor, TeamMember target, TeamRole role)
    {
        var actorRole = (await FindMembershipAsync(target.TeamId, actor.Id))?.Role;

        if (actorRole is not TeamRole.Owner || target.UserId == actor.Id || role is TeamRole.Owner)
            return SetMemberRoleResult.NotAllowed;

        target.Role = role;
        await _members.SaveChangesAsync();

        Log.Information("User {UserId} made user {TargetId} {Role} in team {TeamId}",
            actor.Id, target.UserId, role, target.TeamId);

        return SetMemberRoleResult.Changed;
    }

    public async Task<RemoveMemberResult> RemoveMemberAsync(ApplicationUser actor, TeamMember target)
    {
        var actorRole = (await FindMembershipAsync(target.TeamId, actor.Id))?.Role;

        var allowed = target.UserId != actor.Id
                      && target.Role is not TeamRole.Owner
                      && (actorRole is TeamRole.Owner || (actorRole is TeamRole.Admin && target.Role is TeamRole.Member));

        if (!allowed)
            return RemoveMemberResult.NotAllowed;

        await _members.RemoveEntryAsync(target);
        await _members.SaveChangesAsync();

        Log.Information("User {UserId} removed user {TargetId} from team {TeamId}", actor.Id, target.UserId, target.TeamId);

        return RemoveMemberResult.Removed;
    }

    /// <summary>
    /// Takes the user out of all their teams before the account is deleted. A team the user owns goes to its
    /// longest-serving admin, else its longest-serving member, and is deleted when nobody else is left.
    /// </summary>
    public async Task RemoveUserFromTeamsAsync(ApplicationUser user)
    {
        var memberships = await _members.GetAll().Where(m => m.UserId == user.Id).ToListAsync();

        foreach (var membership in memberships)
        {
            if (membership.Role is not TeamRole.Owner)
            {
                await _members.RemoveEntryAsync(membership);
                continue;
            }

            var successor = await _members.GetAll()
                .Where(m => m.TeamId == membership.TeamId && m.UserId != user.Id)
                .OrderByDescending(m => m.Role)
                .ThenBy(m => m.JoinedOn)
                .FirstOrDefaultAsync();

            if (successor is null)
            {
                await _teams.RemoveAsync(membership.TeamId);
                continue;
            }

            successor.Role = TeamRole.Owner;
            await _members.RemoveEntryAsync(membership);
        }

        await _members.SaveChangesAsync();

        Log.Information("Removed user {UserId} from {TeamCount} team(s) before account deletion", user.Id, memberships.Count);
    }

    private static (string Name, string? Description) Normalize(string name, string? description)
    {
        return (name.Trim(), string.IsNullOrWhiteSpace(description) ? null : description.Trim());
    }

    private async Task<TeamMember?> FindMembershipAsync(int teamId, string userId)
    {
        return await _members.GetAll().FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
    }

    private async Task<bool> HasReachedMembershipLimitAsync(ApplicationUser user)
    {
        if (_settings.TeamsPerUser <= 0)
            return false;

        return await _members.GetAll().CountAsync(m => m.UserId == user.Id) >= _settings.TeamsPerUser;
    }

    private async Task<int> CountOwnedTeamsAsync(ApplicationUser user)
    {
        return await _members.GetAll().CountAsync(m => m.UserId == user.Id && m.Role == TeamRole.Owner);
    }
}

public enum CreateTeamStatus
{
    Created,
    PilotNotLinked,
    InvalidName,
    DescriptionTooLong,
    NameTaken,
    MembershipLimitReached,
    OwnedTeamsLimitReached
}

public record CreateTeamResult(CreateTeamStatus Status, Team? Team = null);

public enum UpdateTeamResult
{
    Updated,
    NotAllowed,
    InvalidName,
    DescriptionTooLong,
    NameTaken
}

public enum RemoveTeamResult
{
    Removed,
    NotOwner
}

public enum InviteToTeamResult
{
    Invited,
    NotAllowed,
    AlreadyMember,
    AlreadyInvited,
    TeamFull,
    InviteeMembershipLimitReached
}

public enum AcceptInviteResult
{
    Accepted,
    NotInvitee,
    TeamFull,
    MembershipLimitReached
}

public enum DeclineInviteResult
{
    Declined,
    NotInvitee
}

public enum LeaveTeamResult
{
    Left,
    NotMember,
    OwnerCannotLeave
}

public enum RevokeInviteResult
{
    Revoked,
    NotAllowed
}

public enum SetMemberRoleResult
{
    Changed,
    NotAllowed
}

public enum RemoveMemberResult
{
    Removed,
    NotAllowed
}
