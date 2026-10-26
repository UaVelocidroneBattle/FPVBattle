using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams.Notifications;

namespace Veloci.Logic.Features.Teams.Services;

public class TeamService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TeamService>();

    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<TeamMembershipRequest> _requests;
    private readonly TeamSettings _settings;
    private readonly IPublisher _publisher;

    public TeamService(
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<TeamMembershipRequest> requests,
        IOptions<TeamSettings> settings,
        IPublisher publisher)
    {
        _teams = teams;
        _members = members;
        _requests = requests;
        _settings = settings.Value;
        _publisher = publisher;
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

        if (await IsInTeamAsync(owner))
            return new CreateTeamResult(CreateTeamStatus.AlreadyInTeam);

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
        await DropPendingOfAsync(owner);
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
        if (await _members.GetAll().AnyAsync(m => m.TeamId == team.Id && m.UserId == invitee.Id))
            return InviteToTeamResult.AlreadyMember;

        var pending = await FindPendingAsync(team.Id, invitee.Id);

        if (pending is { IsJoinRequest: false })
            return InviteToTeamResult.AlreadyInvited;

        if (await IsFullAsync(team.Id))
            return InviteToTeamResult.TeamFull;

        if (await IsInTeamAsync(invitee))
            return InviteToTeamResult.InviteeInAnotherTeam;

        // The pilot has already asked to join, so inviting them means letting them in
        if (pending is not null)
        {
            await ApproveAsync(team, invitee);
            return InviteToTeamResult.JoinRequestApproved;
        }

        await _requests.AddAsync(new TeamMembershipRequest
        {
            TeamId = team.Id,
            UserId = invitee.Id,
            InvitedByUserId = inviter.Id
        });

        await _requests.SaveChangesAsync();

        Log.Information("User {InviterId} invited user {InviteeId} to team {TeamName} ({TeamId})",
            inviter.Id, invitee.Id, team.Name, team.Id);

        await _publisher.Publish(new TeamInviteSentNotification(team, inviter, invitee));

        return InviteToTeamResult.Invited;
    }

    /// <summary>
    /// Makes the user a plain member without an invitation.
    /// </summary>
    public async Task<AddMemberResult> AddMemberAsync(Team team, ApplicationUser user)
    {
        if (await _members.GetAll().AnyAsync(m => m.TeamId == team.Id && m.UserId == user.Id))
            return AddMemberResult.AlreadyMember;

        if (await IsFullAsync(team.Id))
            return AddMemberResult.TeamFull;

        if (await IsInTeamAsync(user))
            return AddMemberResult.InAnotherTeam;

        await JoinAsync(team.Id, user);

        Log.Information("Added user {UserId} to team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        return AddMemberResult.Added;
    }

    public async Task<AcceptInviteResult> AcceptInviteAsync(ApplicationUser user, TeamMembershipRequest invite)
    {
        if (await IsFullAsync(invite.TeamId))
            return AcceptInviteResult.TeamFull;

        // An invite sent earlier stays pending after the user joins another team, so this is checked again here.
        if (await IsInTeamAsync(user))
            return AcceptInviteResult.AlreadyInTeam;

        await JoinAsync(invite.TeamId, user);

        Log.Information("User {UserId} accepted the invite to team {TeamId}", user.Id, invite.TeamId);

        return AcceptInviteResult.Accepted;
    }

    /// <summary>
    /// Asks a public team to let the user in. A user the team has already invited answers the invite instead.
    /// </summary>
    public async Task<RequestToJoinResult> RequestToJoinAsync(ApplicationUser user, Team team)
    {
        if (user.PilotId is null)
            return RequestToJoinResult.PilotNotLinked;

        if (await IsInTeamAsync(user))
            return RequestToJoinResult.AlreadyInTeam;

        if (await _members.GetAll().CountAsync(m => m.TeamId == team.Id) < _settings.MinPublicTeamSize)
            return RequestToJoinResult.TeamNotPublic;

        var pending = await FindPendingAsync(team.Id, user.Id);

        if (pending is not null)
            return pending.IsJoinRequest ? RequestToJoinResult.AlreadyRequested : RequestToJoinResult.AlreadyInvited;

        if (await IsFullAsync(team.Id))
            return RequestToJoinResult.TeamFull;

        if (await _requests.GetAll().JoinRequests().CountAsync(i => i.UserId == user.Id) >= _settings.MaxPendingJoinRequests)
            return RequestToJoinResult.TooManyRequests;

        await _requests.AddAsync(new TeamMembershipRequest
        {
            TeamId = team.Id,
            UserId = user.Id
        });

        await _requests.SaveChangesAsync();

        Log.Information("User {UserId} asked to join team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        await _publisher.Publish(new TeamJoinRequestedNotification(team, user));

        return RequestToJoinResult.Requested;
    }

    public async Task<ApproveJoinRequestResult> ApproveJoinRequestAsync(TeamMembershipRequest request)
    {
        if (await IsFullAsync(request.TeamId))
            return ApproveJoinRequestResult.TeamFull;

        if (await IsInTeamAsync(request.User))
            return ApproveJoinRequestResult.AlreadyInTeam;

        await ApproveAsync(request.Team, request.User);

        return ApproveJoinRequestResult.Approved;
    }

    public async Task RejectJoinRequestAsync(TeamMembershipRequest request)
    {
        await _requests.RemoveEntryAsync(request);
        await _requests.SaveChangesAsync();

        Log.Information("Rejected the join request of user {UserId} to team {TeamId}", request.UserId, request.TeamId);
    }

    public async Task CancelJoinRequestAsync(TeamMembershipRequest request)
    {
        await _requests.RemoveEntryAsync(request);
        await _requests.SaveChangesAsync();

        Log.Information("User {UserId} cancelled the join request to team {TeamId}", request.UserId, request.TeamId);
    }

    public async Task DeclineInviteAsync(ApplicationUser user, TeamMembershipRequest invite)
    {
        await _requests.RemoveEntryAsync(invite);
        await _requests.SaveChangesAsync();

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

    public async Task RevokeInviteAsync(TeamMembershipRequest invite)
    {
        await _requests.RemoveEntryAsync(invite);
        await _requests.SaveChangesAsync();

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
    /// Takes the user out of their team before the account is deleted. A team the user owns goes to its
    /// longest-serving admin, else its longest-serving member, and is deleted when nobody else is left.
    /// </summary>
    public async Task RemoveUserFromTeamAsync(ApplicationUser user)
    {
        var membership = await _members.GetAll().FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (membership is null)
            return;

        if (membership.Role is TeamRole.Owner)
        {
            var successor = await _members.GetAll()
                .Where(m => m.TeamId == membership.TeamId && m.UserId != user.Id)
                .OrderByDescending(m => m.Role)
                .ThenBy(m => m.JoinedOn)
                .FirstOrDefaultAsync();

            if (successor is null)
            {
                await RemoveAsync(membership.Team);
                return;
            }

            successor.Role = TeamRole.Owner;
        }

        await _members.RemoveEntryAsync(membership);
        await _members.SaveChangesAsync();

        Log.Information("Removed user {UserId} from team {TeamId} before account deletion", user.Id, membership.TeamId);
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

    private async Task ApproveAsync(Team team, ApplicationUser user)
    {
        await JoinAsync(team.Id, user);

        Log.Information("Approved the join request of user {UserId} to team {TeamName} ({TeamId})", user.Id, team.Name, team.Id);

        await _publisher.Publish(new TeamJoinRequestApprovedNotification(team, user));
    }

    /// <summary>
    /// Adds the user as a plain member. Their other invitations and join requests are dropped: a user is in one team only.
    /// </summary>
    private async Task JoinAsync(int teamId, ApplicationUser user)
    {
        await _members.AddAsync(new TeamMember
        {
            TeamId = teamId,
            UserId = user.Id,
            Role = TeamRole.Member
        });

        await DropPendingOfAsync(user);
        await _members.SaveChangesAsync();
    }

    private async Task DropPendingOfAsync(ApplicationUser user)
    {
        var pending = await _requests.GetAll().Where(i => i.UserId == user.Id).ToListAsync();

        foreach (var entry in pending)
            await _requests.RemoveEntryAsync(entry);
    }

    private async Task<TeamMembershipRequest?> FindPendingAsync(int teamId, string userId)
    {
        return await _requests.GetAll().FirstOrDefaultAsync(i => i.TeamId == teamId && i.UserId == userId);
    }

    private async Task<bool> IsInTeamAsync(ApplicationUser user)
    {
        return await _members.GetAll().AnyAsync(m => m.UserId == user.Id);
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
    AlreadyInTeam
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
    InviteeInAnotherTeam,
    JoinRequestApproved
}

public enum AddMemberResult
{
    Added,
    AlreadyMember,
    TeamFull,
    InAnotherTeam
}

public enum AcceptInviteResult
{
    Accepted,
    TeamFull,
    AlreadyInTeam
}

public enum RequestToJoinResult
{
    Requested,
    PilotNotLinked,
    AlreadyInTeam,
    TeamNotPublic,
    AlreadyRequested,
    AlreadyInvited,
    TeamFull,
    TooManyRequests
}

public enum ApproveJoinRequestResult
{
    Approved,
    TeamFull,
    AlreadyInTeam
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
