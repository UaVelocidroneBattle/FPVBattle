using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams;
using Veloci.Logic.Features.Teams.Services;
using Veloci.Logic.Helpers;

namespace Veloci.Web.Controllers.Admin.Teams;

/// <summary>
/// Administers teams on behalf of nobody in particular, so unlike the public API it skips <see cref="TeamPermissions"/>.
/// The team limits still apply.
/// </summary>
public class TeamManagementController : AdminControllerBase
{
    private readonly IRepository<Team> _teams;
    private readonly IRepository<TeamMember> _members;
    private readonly IRepository<TeamInvite> _invites;
    private readonly IRepository<ApplicationUser> _users;
    private readonly IRepository<Pilot> _pilots;
    private readonly TeamService _teamService;
    private readonly TeamSettings _settings;

    public TeamManagementController(
        IRepository<Team> teams,
        IRepository<TeamMember> members,
        IRepository<TeamInvite> invites,
        IRepository<ApplicationUser> users,
        IRepository<Pilot> pilots,
        TeamService teamService,
        IOptions<TeamSettings> settings)
    {
        _teams = teams;
        _members = members;
        _invites = invites;
        _users = users;
        _pilots = pilots;
        _teamService = teamService;
        _settings = settings.Value;
    }

    public async Task<IActionResult> Index()
    {
        var rows = await _teams.GetAll()
            .OrderBy(t => t.Name)
            .Select(t => new TeamRow
            {
                Id = t.Id,
                Name = t.Name,
                OwnerPilotName = t.Members
                    .Where(m => m.Role == TeamRole.Owner)
                    .Select(m => m.User.Pilot != null ? m.User.Pilot.Name : null)
                    .FirstOrDefault(),
                MemberCount = t.Members.Count,
                InviteCount = t.Invites.Count,
                CreatedOn = t.CreatedOn
            })
            .ToListAsync();

        return View(new TeamsViewModel
        {
            Teams = rows,
            MaxTeamSize = _settings.MaxTeamSize,
            OwnerCandidates = await FindOwnerCandidatesAsync()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var team = await _teams.FindAsync(id);

        if (team is null)
            return NotFound();

        var members = await _members.GetAll()
            .Where(m => m.TeamId == id)
            .OrderByDescending(m => m.Role)
            .ThenBy(m => m.JoinedOn)
            .Select(m => new TeamMemberRow
            {
                UserId = m.UserId,
                PilotName = m.User.Pilot != null ? m.User.Pilot.Name : null,
                Role = m.Role,
                JoinedOn = m.JoinedOn
            })
            .ToListAsync();

        var invites = await _invites.GetAll()
            .Where(i => i.TeamId == id)
            .OrderBy(i => i.InvitedOn)
            .Select(i => new TeamInviteRow
            {
                UserId = i.UserId,
                PilotName = i.User.Pilot != null ? i.User.Pilot.Name : null,
                InvitedBy = i.InvitedByUser.Pilot != null ? i.InvitedByUser.Pilot.Name : null,
                InvitedOn = i.InvitedOn
            })
            .ToListAsync();

        return View(new TeamDetailsViewModel
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            CreatedOn = team.CreatedOn,
            MaxTeamSize = _settings.MaxTeamSize,
            Members = members,
            Invites = invites,
            MemberCandidates = await FindMemberCandidatesAsync(id, members.Count)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description, string ownerPilotName)
    {
        var owner = await FindUserByPilotNameAsync(ownerPilotName);

        if (owner is null)
            return RedirectToAction(nameof(Index));

        var result = await _teamService.CreateAsync(owner, name ?? string.Empty, description);

        var error = result.Status switch
        {
            CreateTeamStatus.InvalidName => TeamMessages.InvalidName,
            CreateTeamStatus.DescriptionTooLong => TeamMessages.DescriptionTooLong,
            CreateTeamStatus.NameTaken => TeamMessages.NameTaken,
            CreateTeamStatus.MembershipLimitReached => "This pilot is already in the maximum number of teams",
            CreateTeamStatus.OwnedTeamsLimitReached => "This pilot has reached the limit of teams they can own",
            _ => null
        };

        if (error is not null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Team created";

        return RedirectToAction(nameof(Details), new { id = result.Team!.Id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name, string? description)
    {
        var team = await _teams.FindAsync(id);

        if (team is null)
            return NotFound();

        var result = await _teamService.UpdateAsync(team, name ?? string.Empty, description);

        var error = result switch
        {
            UpdateTeamResult.InvalidName => TeamMessages.InvalidName,
            UpdateTeamResult.DescriptionTooLong => TeamMessages.DescriptionTooLong,
            UpdateTeamResult.NameTaken => TeamMessages.NameTaken,
            _ => null
        };

        Report(error, "Team updated");

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _teams.FindAsync(id);

        if (team is null)
            return NotFound();

        await _teamService.RemoveAsync(team);

        TempData["Success"] = "Team deleted";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int id, string pilotName)
    {
        var team = await _teams.FindAsync(id);

        if (team is null)
            return NotFound();

        var user = await FindUserByPilotNameAsync(pilotName);

        if (user is not null)
        {
            var result = await _teamService.AddMemberAsync(team, user);

            var error = result switch
            {
                AddMemberResult.AlreadyMember => "This pilot is already a member of the team",
                AddMemberResult.TeamFull => "The team is full",
                AddMemberResult.MembershipLimitReached => "This pilot is already in the maximum number of teams",
                _ => null
            };

            Report(error, "Pilot added to the team");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int id, string userId, TeamRole role)
    {
        var member = await FindMemberAsync(id, userId);

        if (member is null)
            return NotFound();

        var result = await _teamService.SetRoleAsync(member, role);

        Report(result is SetMemberRoleResult.OwnerRoleIsFixed ? "The owner's role cannot be changed" : null, "Role changed");

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id, string userId)
    {
        var member = await FindMemberAsync(id, userId);

        if (member is null)
            return NotFound();

        var result = await _teamService.RemoveMemberAsync(member);

        Report(result is RemoveMemberResult.OwnerCannotBeRemoved ? "The owner cannot be removed. Delete the team instead" : null,
            "Member removed");

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeInvite(int id, string userId)
    {
        var invite = await _invites.GetAll().FirstOrDefaultAsync(i => i.TeamId == id && i.UserId == userId);

        if (invite is null)
            return NotFound();

        await _teamService.RevokeInviteAsync(invite);

        TempData["Success"] = "Invite revoked";

        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>Pilots linked to an account that is under both the membership and the owned-teams limit.</summary>
    private async Task<List<string>> FindOwnerCandidatesAsync()
    {
        return await UsersWithRoomForAnotherTeam()
            .Where(u => _members.GetAll().Count(m => m.UserId == u.Id && m.Role == TeamRole.Owner) < _settings.MaxOwnedTeamsPerUser)
            .Select(u => u.Pilot!.Name)
            .OrderBy(name => name)
            .ToListAsync();
    }

    /// <summary>Pilots linked to an account that can join the team now: not in it yet and under the membership limit.</summary>
    private async Task<List<string>> FindMemberCandidatesAsync(int teamId, int memberCount)
    {
        if (_settings.MaxTeamSize > 0 && memberCount >= _settings.MaxTeamSize)
            return [];

        return await UsersWithRoomForAnotherTeam()
            .Where(u => !_members.GetAll().Any(m => m.TeamId == teamId && m.UserId == u.Id))
            .Select(u => u.Pilot!.Name)
            .OrderBy(name => name)
            .ToListAsync();
    }

    private IQueryable<ApplicationUser> UsersWithRoomForAnotherTeam()
    {
        var users = _users.GetAll().Where(u => u.Pilot != null);

        if (_settings.TeamsPerUser > 0)
            users = users.Where(u => _members.GetAll().Count(m => m.UserId == u.Id) < _settings.TeamsPerUser);

        return users;
    }

    private async Task<TeamMember?> FindMemberAsync(int teamId, string userId)
    {
        return await _members.GetAll().FirstOrDefaultAsync(m => m.TeamId == teamId && m.UserId == userId);
    }

    /// <summary>The account linked to the pilot. Leaves an error message for the page when there is none.</summary>
    private async Task<ApplicationUser?> FindUserByPilotNameAsync(string? pilotName)
    {
        var pilot = string.IsNullOrWhiteSpace(pilotName)
            ? null
            : await _pilots.GetAll().FindByNameAsync(pilotName.Trim());

        if (pilot is null)
        {
            TempData["Error"] = $"{TeamMessages.PilotNotFound} (names are case sensitive)";
            return null;
        }

        var user = await _users.GetAll().FirstOrDefaultAsync(u => u.PilotId == pilot.Id);

        if (user is null)
            TempData["Error"] = TeamMessages.PilotNotRegistered;

        return user;
    }

    private void Report(string? error, string success)
    {
        if (error is null)
            TempData["Success"] = success;
        else
            TempData["Error"] = error;
    }
}
