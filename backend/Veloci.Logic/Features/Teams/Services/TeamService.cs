using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Features.Teams.Services;

/// <summary>
/// Team operations and the rules that hold for everyone. Who is allowed to run an operation is decided by the
/// caller, see <see cref="TeamPermissions"/>.
/// </summary>
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

    public async Task<CreateTeamResult> CreateAsync(ApplicationUser owner, string name, string tag, string? description)
    {
        if (owner.PilotId is null)
            return new CreateTeamResult(CreateTeamStatus.PilotNotLinked);

        var (normalizedName, normalizedDescription) = Normalize(name, description);
        var normalizedTag = NormalizeTag(tag);

        if (normalizedName.Length is 0 or > Team.NameMaxLength)
            return new CreateTeamResult(CreateTeamStatus.InvalidName);

        if (normalizedDescription?.Length > Team.DescriptionMaxLength)
            return new CreateTeamResult(CreateTeamStatus.DescriptionTooLong);

        if (!IsValidTag(normalizedTag))
            return new CreateTeamResult(CreateTeamStatus.InvalidTag);

        if (await HasReachedMembershipLimitAsync(owner))
            return new CreateTeamResult(CreateTeamStatus.MembershipLimitReached);

        if (await CountOwnedTeamsAsync(owner) >= _settings.MaxOwnedTeamsPerUser)
            return new CreateTeamResult(CreateTeamStatus.OwnedTeamsLimitReached);

        if (await _teams.GetAll().AnyAsync(t => t.Name == normalizedName))
            return new CreateTeamResult(CreateTeamStatus.NameTaken);

        if (IsBlacklistedTag(normalizedTag))
            return new CreateTeamResult(CreateTeamStatus.TagNotAllowed);

        if (await _teams.GetAll().AnyAsync(t => t.Tag == normalizedTag))
            return new CreateTeamResult(CreateTeamStatus.TagTaken);

        var team = new Team
        {
            Name = normalizedName,
            Tag = normalizedTag,
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

    public async Task<UpdateTeamResult> UpdateAsync(Team team, string name, string tag, string? description)
    {
        var (normalizedName, normalizedDescription) = Normalize(name, description);
        var normalizedTag = NormalizeTag(tag);

        if (normalizedName.Length is 0 or > Team.NameMaxLength)
            return UpdateTeamResult.InvalidName;

        if (normalizedDescription?.Length > Team.DescriptionMaxLength)
            return UpdateTeamResult.DescriptionTooLong;

        if (!IsValidTag(normalizedTag))
            return UpdateTeamResult.InvalidTag;

        if (await _teams.GetAll().AnyAsync(t => t.Id != team.Id && t.Name == normalizedName))
            return UpdateTeamResult.NameTaken;

        // A tag blacklisted after the team took it stays usable, so the team can still be edited.
        var keepsOwnTag = string.Equals(team.Tag, normalizedTag, StringComparison.OrdinalIgnoreCase);

        if (!keepsOwnTag && IsBlacklistedTag(normalizedTag))
            return UpdateTeamResult.TagNotAllowed;

        if (await _teams.GetAll().AnyAsync(t => t.Id != team.Id && t.Tag == normalizedTag))
            return UpdateTeamResult.TagTaken;

        team.Name = normalizedName;
        team.Tag = normalizedTag;
        team.Description = normalizedDescription;
        await _teams.SaveChangesAsync();

        Log.Information("Updated team {TeamName} ({TeamId})", team.Name, team.Id);

        return UpdateTeamResult.Updated;
    }

    public async Task RemoveAsync(Team team)
    {
        await _teams.RemoveAsync(team.Id);
        await _teams.SaveChangesAsync();

        Log.Information("Removed team {TeamName} ({TeamId})", team.Name, team.Id);
    }

    public async Task<InviteToTeamResult> InviteAsync(ApplicationUser inviter, Team team, ApplicationUser invitee)
    {
        var teamMembers = _members.GetAll().Where(m => m.TeamId == team.Id);

        if (await teamMembers.AnyAsync(m => m.UserId == invitee.Id))
            return InviteToTeamResult.AlreadyMember;

        if (await _invites.GetAll().AnyAsync(i => i.TeamId == team.Id && i.UserId == invitee.Id))
            return InviteToTeamResult.AlreadyInvited;

        if (await IsFullAsync(team.Id))
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

    /// <summary>
    /// Makes the user a plain member without an invitation. A pending invite of the user to this team is dropped.
    /// </summary>
    public async Task<AddMemberResult> AddMemberAsync(Team team, ApplicationUser user)
    {
        if (await _members.GetAll().AnyAsync(m => m.TeamId == team.Id && m.UserId == user.Id))
            return AddMemberResult.AlreadyMember;

        if (await IsFullAsync(team.Id))
            return AddMemberResult.TeamFull;

        if (await HasReachedMembershipLimitAsync(user))
            return AddMemberResult.MembershipLimitReached;

        await _members.AddAsync(new TeamMember
        {
            TeamId = team.Id,
            UserId = user.Id,
            Role = TeamRole.Member
        });

        var invite = await _invites.GetAll().FirstOrDefaultAsync(i => i.TeamId == team.Id && i.UserId == user.Id);

        if (invite is not null)
            await _invites.RemoveEntryAsync(invite);

        await _members.SaveChangesAsync();

        Log.Information("Added user {UserId} to team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        return AddMemberResult.Added;
    }

    public async Task<AcceptInviteResult> AcceptInviteAsync(ApplicationUser user, TeamInvite invite)
    {
        if (await IsFullAsync(invite.TeamId))
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

    public async Task DeclineInviteAsync(ApplicationUser user, TeamInvite invite)
    {
        await _invites.RemoveEntryAsync(invite);
        await _invites.SaveChangesAsync();

        Log.Information("User {UserId} declined the invite to team {TeamId}", user.Id, invite.TeamId);
    }

    public async Task<LeaveTeamResult> LeaveAsync(ApplicationUser user, Team team)
    {
        var membership = await _members.GetAll().FirstOrDefaultAsync(m => m.TeamId == team.Id && m.UserId == user.Id);

        if (membership is null)
            return LeaveTeamResult.NotMember;

        if (membership.Role is TeamRole.Owner)
            return LeaveTeamResult.OwnerCannotLeave;

        await _members.RemoveEntryAsync(membership);
        await _members.SaveChangesAsync();

        Log.Information("User {UserId} left team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        return LeaveTeamResult.Left;
    }

    public async Task RevokeInviteAsync(TeamInvite invite)
    {
        await _invites.RemoveEntryAsync(invite);
        await _invites.SaveChangesAsync();

        Log.Information("Revoked the invite of user {InviteeId} to team {TeamId}", invite.UserId, invite.TeamId);
    }

    public async Task<SetMemberRoleResult> SetRoleAsync(TeamMember target, TeamRole role)
    {
        if (target.Role is TeamRole.Owner || role is TeamRole.Owner)
            return SetMemberRoleResult.OwnerRoleIsFixed;

        target.Role = role;
        await _members.SaveChangesAsync();

        Log.Information("Made user {TargetId} {Role} in team {TeamId}", target.UserId, role, target.TeamId);

        return SetMemberRoleResult.Changed;
    }

    public async Task<RemoveMemberResult> RemoveMemberAsync(TeamMember target)
    {
        if (target.Role is TeamRole.Owner)
            return RemoveMemberResult.OwnerCannotBeRemoved;

        await _members.RemoveEntryAsync(target);
        await _members.SaveChangesAsync();

        Log.Information("Removed user {TargetId} from team {TeamId}", target.UserId, target.TeamId);

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

    private static string NormalizeTag(string tag)
    {
        return tag.Trim().ToUpperInvariant();
    }

    private static bool IsValidTag(string normalizedTag)
    {
        return normalizedTag.Length is >= Team.TagMinLength and <= Team.TagMaxLength
               && normalizedTag.All(char.IsAsciiLetterOrDigit);
    }

    private bool IsBlacklistedTag(string normalizedTag)
    {
        return _settings.BlacklistedTags.Contains(normalizedTag, StringComparer.OrdinalIgnoreCase);
    }

    private async Task<bool> IsFullAsync(int teamId)
    {
        if (_settings.MaxTeamSize <= 0)
            return false;

        return await _members.GetAll().CountAsync(m => m.TeamId == teamId) >= _settings.MaxTeamSize;
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
    InvalidTag,
    TagNotAllowed,
    TagTaken,
    MembershipLimitReached,
    OwnedTeamsLimitReached
}

public record CreateTeamResult(CreateTeamStatus Status, Team? Team = null);

public enum UpdateTeamResult
{
    Updated,
    InvalidName,
    DescriptionTooLong,
    NameTaken,
    InvalidTag,
    TagNotAllowed,
    TagTaken
}

public enum InviteToTeamResult
{
    Invited,
    AlreadyMember,
    AlreadyInvited,
    TeamFull,
    InviteeMembershipLimitReached
}

public enum AddMemberResult
{
    Added,
    AlreadyMember,
    TeamFull,
    MembershipLimitReached
}

public enum AcceptInviteResult
{
    Accepted,
    TeamFull,
    MembershipLimitReached
}

public enum LeaveTeamResult
{
    Left,
    NotMember,
    OwnerCannotLeave
}

public enum SetMemberRoleResult
{
    Changed,
    OwnerRoleIsFixed
}

public enum RemoveMemberResult
{
    Removed,
    OwnerCannotBeRemoved
}
