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
/// The team rules still apply: one team per user and the team size limit.
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
                Tag = t.Tag,
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
            OwnerCandidates = await FindPilotsWithoutTeamAsync()
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
            Tag = team.Tag,
            Description = team.Description,
            CreatedOn = team.CreatedOn,
            MaxTeamSize = _settings.MaxTeamSize,
            Members = members,
            Invites = invites,
            MemberCandidates = await FindMemberCandidatesAsync(members.Count)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string tag, string? description, string ownerPilotName)
    {
        var owner = await FindUserByPilotNameAsync(ownerPilotName);

        if (owner is null)
            return RedirectToAction(nameof(Index));

        var result = await _teamService.CreateAsync(owner, name ?? string.Empty, tag ?? string.Empty, description);

        var error = result.Status switch
        {
            CreateTeamStatus.InvalidName => TeamMessages.InvalidName,
            CreateTeamStatus.DescriptionTooLong => TeamMessages.DescriptionTooLong,
            CreateTeamStatus.InvalidTag => TeamMessages.InvalidTag,
            CreateTeamStatus.TagNotAllowed => TeamMessages.TagNotAllowed,
            CreateTeamStatus.NameTaken => TeamMessages.NameTaken,
            CreateTeamStatus.TagTaken => TeamMessages.TagTaken,
            CreateTeamStatus.AlreadyInTeam => "This pilot is already in a team",
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
    public async Task<IActionResult> Edit(int id, string name, string tag, string? description)
    {
        var team = await _teams.FindAsync(id);

        if (team is null)
            return NotFound();

        var result = await _teamService.UpdateAsync(team, name ?? string.Empty, tag ?? string.Empty, description);

        var error = result switch
        {
            UpdateTeamResult.InvalidName => TeamMessages.InvalidName,
            UpdateTeamResult.DescriptionTooLong => TeamMessages.DescriptionTooLong,
            UpdateTeamResult.InvalidTag => TeamMessages.InvalidTag,
            UpdateTeamResult.TagNotAllowed => TeamMessages.TagNotAllowed,
            UpdateTeamResult.NameTaken => TeamMessages.NameTaken,
            UpdateTeamResult.TagTaken => TeamMessages.TagTaken,
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
                AddMemberResult.InAnotherTeam => "This pilot is already in another team",
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

    /// <summary>Pilots that can join the team now: in no team yet, while the team has room.</summary>
    private async Task<List<string>> FindMemberCandidatesAsync(int memberCount)
    {
        if (_settings.MaxTeamSize > 0 && memberCount >= _settings.MaxTeamSize)
            return [];

        return await FindPilotsWithoutTeamAsync();
    }

    /// <summary>Pilots linked to an account that is in no team yet.</summary>
    private Task<List<string>> FindPilotsWithoutTeamAsync()
    {
        return _users.GetAll()
            .Where(u => u.Pilot != null && !_members.GetAll().Any(m => m.UserId == u.Id))
            .Select(u => u.Pilot!.Name)
            .OrderBy(name => name)
            .ToListAsync();
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
