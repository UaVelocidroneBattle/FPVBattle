using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams;
using Veloci.Logic.Features.Teams.Services;

namespace Veloci.Web.Controllers.Api.Teams;

[ApiController]
[Route("/api/teams/[action]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TeamsController : ControllerBase
{
    private const int SearchResultsLimit = 10;
    private const int SuggestionsLimit = 8;
    private const int MinSuggestionQueryLength = 2;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TeamService _teamService;
    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<TeamInvite> _invites;
    private readonly IRepository<Pilot> _pilots;
    private readonly TeamSettings _settings;

    public TeamsController(
        UserManager<ApplicationUser> userManager,
        TeamService teamService,
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<TeamInvite> invites,
        IRepository<Pilot> pilots,
        IOptions<TeamSettings> settings)
    {
        _userManager = userManager;
        _teamService = teamService;
        _teams = teams;
        _members = members;
        _invites = invites;
        _pilots = pilots;
        _settings = settings.Value;
    }

    /// <summary>
    /// The team settings, plus the teams and invitations of the signed-in user. A visitor gets them empty.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<TeamsOverviewModel>> Overview()
    {
        var userId = _userManager.GetUserId(User);

        return new TeamsOverviewModel
        {
            MyTeams = userId is null ? [] : await GetTeamsOfUserAsync(userId),
            Invitations = userId is null ? [] : await GetInvitationsOfUserAsync(userId),
            MaxTeamSize = _settings.MaxTeamSize,
            TeamsPerUser = _settings.TeamsPerUser
        };
    }

    /// <summary>
    /// The biggest teams, optionally filtered by name. A signed-in user does not get their own teams here.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<TeamSummaryModel>>> Search([FromQuery] string? query)
    {
        var userId = _userManager.GetUserId(User);

        var teams = _teams.GetAll();

        if (userId is not null)
            teams = teams.Where(t => t.Members.All(m => m.UserId != userId));

        if (!string.IsNullOrWhiteSpace(query))
            teams = teams.Where(t => EF.Functions.Like(t.Name, TeamModelMapper.ContainsPattern(query.Trim()), "\\"));

        return await teams
            .OrderByDescending(t => t.Members.Count)
            .ThenBy(t => t.Name)
            .Take(SearchResultsLimit)
            .Select(t => new TeamSummaryModel { Id = t.Id, Name = t.Name, MemberCount = t.Members.Count })
            .ToListAsync();
    }

    /// <summary>
    /// Pilots whose name contains the text and who can be invited to the team right now. Only for the owner and admins.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PilotSuggestionModel>>> InviteCandidates([FromQuery] int teamId, [FromQuery] string? query)
    {
        var userId = _userManager.GetUserId(User);

        if (userId is null)
            return Unauthorized();

        if (!TeamPermissions.CanInvite(await FindMemberAsync(teamId, userId)))
            return Forbid();

        if (query is null || query.Trim().Length < MinSuggestionQueryLength)
            return new List<PilotSuggestionModel>();

        var pattern = TeamModelMapper.ContainsPattern(query.Trim());
        var membershipLimit = _settings.TeamsPerUser;

        var candidates = _userManager.Users
            .Where(u => u.Pilot != null && EF.Functions.Like(u.Pilot.Name, pattern, "\\"))
            .Where(u => !_members.GetAll().Any(m => m.TeamId == teamId && m.UserId == u.Id))
            .Where(u => !_invites.GetAll().Any(i => i.TeamId == teamId && i.UserId == u.Id));

        if (membershipLimit > 0)
            candidates = candidates.Where(u => _members.GetAll().Count(m => m.UserId == u.Id) < membershipLimit);

        return await candidates
            .OrderBy(u => u.Pilot!.Name)
            .Take(SuggestionsLimit)
            .Select(u => new PilotSuggestionModel { Name = u.Pilot!.Name, Country = u.Pilot.Country })
            .ToListAsync();
    }

    /// <summary>
    /// A team with its members. A visitor has no role in it and sees no pending invites.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<TeamDetailsModel>> Details([FromQuery] int teamId)
    {
        var userId = _userManager.GetUserId(User);

        var team = await _teams.FindAsync(teamId);

        if (team is null)
            return NotFound(TeamMessages.TeamNotFound);

        var members = await _members.GetAll()
            .Where(m => m.TeamId == teamId)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedOn)
            .Select(m => new TeamMemberModel
            {
                UserId = m.UserId,
                Role = m.Role,
                JoinedOn = m.JoinedOn,
                PilotName = m.User.Pilot != null ? m.User.Pilot.Name : null,
                Country = m.User.Pilot != null ? m.User.Pilot.Country : null
            })
            .ToListAsync();

        var myRole = members.FirstOrDefault(m => m.UserId == userId)?.Role;
        var pendingInvites = new List<TeamPendingInviteModel>();

        if (myRole is TeamRole.Owner or TeamRole.Admin)
        {
            pendingInvites = await _invites.GetAll()
                .Where(i => i.TeamId == teamId)
                .OrderBy(i => i.InvitedOn)
                .Select(i => new TeamPendingInviteModel
                {
                    UserId = i.UserId,
                    PilotName = i.User.Pilot != null ? i.User.Pilot.Name : null,
                    Country = i.User.Pilot != null ? i.User.Pilot.Country : null,
                    InvitedBy = i.InvitedByUser.Pilot != null ? i.InvitedByUser.Pilot.Name : null,
                    InvitedOn = i.InvitedOn
                })
                .ToListAsync();
        }

        return new TeamDetailsModel
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            MemberCount = members.Count,
            MyRole = myRole,
            Members = members,
            PendingInvites = pendingInvites
        };
    }

    [HttpPost]
    public async Task<ActionResult<TeamModel>> Create([FromBody] CreateTeamModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var result = await _teamService.CreateAsync(user, model.Name, model.Description);

        return result.Status switch
        {
            CreateTeamStatus.PilotNotLinked => BadRequest("Link your Velocidrone pilot to your account before creating a team"),
            CreateTeamStatus.InvalidName => BadRequest(TeamMessages.InvalidName),
            CreateTeamStatus.DescriptionTooLong => BadRequest(TeamMessages.DescriptionTooLong),
            CreateTeamStatus.NameTaken => Conflict(TeamMessages.NameTaken),
            CreateTeamStatus.MembershipLimitReached => Conflict("You are already in the maximum number of teams. Leave a team to create a new one"),
            CreateTeamStatus.OwnedTeamsLimitReached => Conflict("You have reached the limit of teams you can own"),
            _ => new TeamModel
            {
                Id = result.Team!.Id,
                Name = result.Team.Name,
                Description = result.Team.Description
            }
        };
    }

    /// <summary>
    /// Changes the name and description of the team. Only the owner and admins can do it.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Update([FromBody] UpdateTeamModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var team = await _teams.FindAsync(model.TeamId);

        if (team is null)
            return NotFound(TeamMessages.TeamNotFound);

        if (!TeamPermissions.CanEditTeam(await FindMemberAsync(team.Id, user.Id)))
            return Forbid();

        var result = await _teamService.UpdateAsync(team, model.Name, model.Description);

        return result switch
        {
            UpdateTeamResult.InvalidName => BadRequest(TeamMessages.InvalidName),
            UpdateTeamResult.DescriptionTooLong => BadRequest(TeamMessages.DescriptionTooLong),
            UpdateTeamResult.NameTaken => Conflict(TeamMessages.NameTaken),
            _ => NoContent()
        };
    }

    [HttpPost]
    public async Task<ActionResult> Invite([FromBody] InviteModel model)
    {
        var inviter = await _userManager.GetUserAsync(User);

        if (inviter is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(model.PilotName))
            return BadRequest("Pilot name is required");

        var team = await _teams.FindAsync(model.TeamId);

        if (team is null)
            return NotFound(TeamMessages.TeamNotFound);

        if (!TeamPermissions.CanInvite(await FindMemberAsync(team.Id, inviter.Id)))
            return Forbid();

        var pilot = await _pilots.GetAll().FindByNameAsync(model.PilotName);

        if (pilot is null)
            return NotFound(TeamMessages.PilotNotFound);

        var invitee = await _userManager.Users.FirstOrDefaultAsync(u => u.PilotId == pilot.Id);

        if (invitee is null)
            return NotFound(TeamMessages.PilotNotRegistered);

        var result = await _teamService.InviteAsync(inviter, team, invitee);

        return result switch
        {
            InviteToTeamResult.AlreadyMember => Conflict("This pilot is already a member of the team"),
            InviteToTeamResult.AlreadyInvited => Conflict("This pilot has already been invited"),
            InviteToTeamResult.TeamFull => Conflict("The team is full"),
            InviteToTeamResult.InviteeMembershipLimitReached => Conflict("This pilot is already in a team"),
            _ => Ok()
        };
    }

    [HttpPost]
    public async Task<ActionResult> Remove([FromBody] TeamReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var team = await _teams.FindAsync(model.TeamId);

        if (team is null)
            return NotFound(TeamMessages.TeamNotFound);

        if (!TeamPermissions.CanDeleteTeam(await FindMemberAsync(team.Id, user.Id)))
            return Forbid();

        await _teamService.RemoveAsync(team);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Accept([FromBody] TeamReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var invite = await FindInviteAsync(model.TeamId, user.Id);

        if (invite is null)
            return NotFound("Invite not found");

        var result = await _teamService.AcceptInviteAsync(user, invite);

        return result switch
        {
            AcceptInviteResult.TeamFull => Conflict("The team is full"),
            AcceptInviteResult.MembershipLimitReached => Conflict("You are already in the maximum number of teams. Leave a team to join this one"),
            _ => NoContent()
        };
    }

    [HttpPost]
    public async Task<ActionResult> Decline([FromBody] TeamReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var invite = await FindInviteAsync(model.TeamId, user.Id);

        if (invite is null)
            return NotFound("Invite not found");

        await _teamService.DeclineInviteAsync(user, invite);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Leave([FromBody] TeamReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var team = await _teams.FindAsync(model.TeamId);

        if (team is null)
            return NotFound(TeamMessages.TeamNotFound);

        var result = await _teamService.LeaveAsync(user, team);

        return result switch
        {
            LeaveTeamResult.NotMember => NotFound("You are not a member of this team"),
            LeaveTeamResult.OwnerCannotLeave => Conflict("The owner cannot leave the team. Delete the team instead"),
            _ => NoContent()
        };
    }

    [HttpPost]
    public async Task<ActionResult> RevokeInvite([FromBody] TeamMemberReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var invite = await FindInviteAsync(model.TeamId, model.UserId);

        if (invite is null)
            return NotFound("Invite not found");

        if (!TeamPermissions.CanRevokeInvite(await FindMemberAsync(invite.TeamId, user.Id)))
            return Forbid();

        await _teamService.RevokeInviteAsync(invite);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> SetRole([FromBody] SetMemberRoleModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var member = await FindMemberAsync(model.TeamId, model.UserId);

        if (member is null)
            return NotFound("Member not found");

        if (!TeamPermissions.CanChangeRole(await FindMemberAsync(member.TeamId, user.Id), member))
            return Forbid();

        var result = await _teamService.SetRoleAsync(member, model.Role);

        return result is SetMemberRoleResult.OwnerRoleIsFixed ? Forbid() : NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> RemoveMember([FromBody] TeamMemberReferenceModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var member = await FindMemberAsync(model.TeamId, model.UserId);

        if (member is null)
            return NotFound("Member not found");

        if (!TeamPermissions.CanRemoveMember(await FindMemberAsync(member.TeamId, user.Id), member))
            return Forbid();

        var result = await _teamService.RemoveMemberAsync(member);

        return result is RemoveMemberResult.OwnerCannotBeRemoved ? Forbid() : NoContent();
    }

    private async Task<List<TeamSummaryModel>> GetTeamsOfUserAsync(string userId)
    {
        return await _members.GetAll()
            .Where(m => m.UserId == userId)
            .Select(m => new TeamSummaryModel
            {
                Id = m.TeamId,
                Name = m.Team.Name,
                MemberCount = m.Team.Members.Count
            })
            .ToListAsync();
    }

    private async Task<List<TeamInvitationModel>> GetInvitationsOfUserAsync(string userId)
    {
        return await _invites.GetAll()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.InvitedOn)
            .Select(i => new TeamInvitationModel
            {
                TeamId = i.TeamId,
                TeamName = i.Team.Name,
                MemberCount = i.Team.Members.Count,
                InvitedBy = i.InvitedByUser.Pilot != null ? i.InvitedByUser.Pilot.Name : null
            })
            .ToListAsync();
    }

    private async Task<TeamInvite?> FindInviteAsync(int teamId, string userId)
    {
        return await _invites.GetAll().FirstOrDefaultAsync(i => i.TeamId == teamId && i.UserId == userId);
    }

    private async Task<TeamMember?> FindMemberAsync(int teamId, string userId)
    {
        return await _members.GetAll().FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
    }
}
