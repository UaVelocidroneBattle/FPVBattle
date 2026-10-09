using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Veloci.Data;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams;
using Veloci.Logic.Features.Teams.Services;
using Veloci.Web.Controllers.Api.Teams;

namespace Veloci.Tests;

public class TeamsTests : TeamsTestBase
{
    [Fact]
    public async Task Create_MakesTheCreatorTheOwner()
    {
        var owner = AddUser("owner");

        var created = await Controller(owner).Create(new CreateTeamModel { Name = "Alpha", Description = "First" });

        created.Value!.Name.Should().Be("Alpha");
        var overview = (await Controller(owner).Overview()).Value!;
        overview.MyTeams.Should().ContainSingle(t => t.Name == "Alpha" && t.MemberCount == 1);
        (await Controller(owner).Details(created.Value!.Id)).Value!.MyRole.Should().Be(TeamRole.Owner);
    }

    [Fact]
    public async Task Create_RejectsDuplicateNamesIgnoringCase()
    {
        await Controller(AddUser("first")).Create(new CreateTeamModel { Name = "Alpha" });

        var duplicate = await Controller(AddUser("second")).Create(new CreateTeamModel { Name = "aLPHA" });

        duplicate.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_RequiresALinkedPilot()
    {
        var result = await Controller(AddUser("nopilot", withPilot: false)).Create(new CreateTeamModel { Name = "Alpha" });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_LetsOwnersAndAdminsEditTheTeamButNotMembers()
    {
        _settings.TeamsPerUser = 0;
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { admin, member })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }
        await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin));

        (await Controller(member).Update(new UpdateTeamModel { TeamId = teamId, Name = "Hacked" })).Should().BeOfType<ForbidResult>();
        (await Controller(admin).Update(new UpdateTeamModel { TeamId = teamId, Name = "  Beta  ", Description = "New text" }))
            .Should().BeOfType<NoContentResult>();

        var details = (await Controller(owner).Details(teamId)).Value!;
        details.Name.Should().Be("Beta");
        details.Description.Should().Be("New text");
    }

    [Fact]
    public async Task Update_RejectsATakenNameButAllowsChangingTheCaseOfTheOwnName()
    {
        var owner = AddUser("owner");
        await Controller(AddUser("other")).Create(new CreateTeamModel { Name = "Taken" });
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;

        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "tAKEN" })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "ALPHA" })).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "   " })).Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Invite_ThenAccept_MakesTheInviteeAMember()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;

        var invited = await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });
        invited.Should().BeOfType<OkResult>();

        var invitations = (await Controller(invitee).Overview()).Value!.Invitations;
        invitations.Should().ContainSingle(i => i.TeamId == teamId && i.InvitedBy == "pilot_owner" && i.MemberCount == 1);

        (await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<NoContentResult>();

        var details = (await Controller(invitee).Details(teamId)).Value!;
        details.Members.Should().HaveCount(2);
        details.MyRole.Should().Be(TeamRole.Member);
        details.PendingInvites.Should().BeEmpty();
        (await Controller(invitee).Overview()).Value!.Invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task Invite_IsRejectedWhenTheInviteeIsAlreadyInATeam()
    {
        var owner = AddUser("owner");
        var busy = AddUser("busy");
        await Controller(busy).Create(new CreateTeamModel { Name = "Busy team" });
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;

        var result = await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(busy) });

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Accept_IsRejectedWhenTheMembershipLimitIsReached()
    {
        var ownerA = AddUser("ownerA");
        var ownerB = AddUser("ownerB");
        var invitee = AddUser("invitee");
        var teamA = (await Controller(ownerA).Create(new CreateTeamModel { Name = "A" })).Value!.Id;
        var teamB = (await Controller(ownerB).Create(new CreateTeamModel { Name = "B" })).Value!.Id;
        await Controller(ownerA).Invite(new InviteModel { TeamId = teamA, PilotName = PilotNameOf(invitee) });
        await Controller(ownerB).Invite(new InviteModel { TeamId = teamB, PilotName = PilotNameOf(invitee) });

        await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamA });
        var second = await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamB });

        second.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Search_ExcludesMyTeamsAndOrdersBySizeThenFiltersByName()
    {
        var me = AddUser("me");
        await Controller(me).Create(new CreateTeamModel { Name = "Mine" });
        var small = await CreateTeamWithMembers("Small crew", 1);
        var big = await CreateTeamWithMembers("Big crew", 3);
        await CreateTeamWithMembers("100% fast", 2);

        var all = (await Controller(me).Search(null)).Value!;
        all.Select(t => t.Name).Should().Equal("Big crew", "100% fast", "Small crew");

        var filtered = (await Controller(me).Search("CREW")).Value!;
        filtered.Select(t => t.Name).Should().Equal("Big crew", "Small crew");

        var wildcardIsLiteral = (await Controller(me).Search("%")).Value!;
        wildcardIsLiteral.Select(t => t.Name).Should().Equal("100% fast");

        small.Should().NotBe(big);
    }

    [Fact]
    public async Task InviteCandidates_OffersOnlyPilotsThatCanBeInvited()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var invited = AddUser("invited");
        var busy = AddUser("busy");
        AddUser("free");
        AddUser("nopilot", withPilot: false);
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        _settings.TeamsPerUser = 0;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invited) });
        await Controller(busy).Create(new CreateTeamModel { Name = "Busy team" });
        _settings.TeamsPerUser = 1;

        var all = (await Controller(owner).InviteCandidates(teamId, "PILOT_")).Value!;

        all.Select(p => p.Name).Should().Equal("pilot_free");
    }

    [Fact]
    public async Task InviteCandidates_NeedsAtLeastTwoCharactersAndAnOwnerOrAdmin()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        _settings.TeamsPerUser = 0;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });

        (await Controller(owner).InviteCandidates(teamId, "p")).Value!.Should().BeEmpty();
        (await Controller(member).InviteCandidates(teamId, "pilot")).Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Visitors_SeeTheTeamsButNothingPersonal()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha", Description = "Text" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });
        var visitor = Controller(null);

        var overview = (await visitor.Overview()).Value!;
        overview.MyTeams.Should().BeEmpty();
        overview.Invitations.Should().BeEmpty();

        (await visitor.Search(null)).Value!.Select(t => t.Name).Should().Equal("Alpha");

        var details = (await visitor.Details(teamId)).Value!;
        details.Name.Should().Be("Alpha");
        details.Members.Should().ContainSingle();
        details.MyRole.Should().BeNull();
        details.PendingInvites.Should().BeEmpty();
    }

    [Fact]
    public async Task Responses_NeverContainDisplayNamesOrEmails()
    {
        _settings.TeamsPerUser = 0;
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var other = AddUser("other");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });

        var json = System.Text.Json.JsonSerializer.Serialize(new object?[]
        {
            (await Controller(null).Overview()).Value,
            (await Controller(null).Search(null)).Value,
            (await Controller(null).Details(teamId)).Value,
            (await Controller(owner).Overview()).Value,
            (await Controller(owner).Details(teamId)).Value,
            (await Controller(owner).InviteCandidates(teamId, "pilot")).Value,
            (await Controller(invitee).Overview()).Value,
            (await Controller(other).Search(null)).Value
        });

        json.Should().Contain("pilot_owner").And.NotContain("Real Name").And.NotContain("@test.local");
    }

    [Fact]
    public async Task Details_HidesPendingInvitesFromPlainMembers()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var outsider = AddUser("outsider");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(outsider) });

        (await Controller(owner).Details(teamId)).Value!.PendingInvites.Should().ContainSingle();
        (await Controller(member).Details(teamId)).Value!.PendingInvites.Should().BeEmpty();
    }

    [Fact]
    public async Task Leave_LetsMembersGoButNotTheOwner()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });

        (await Controller(owner).Leave(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(member).Leave(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Details(teamId)).Value!.Members.Should().ContainSingle();
    }

    [Fact]
    public async Task Roles_OnlyTheOwnerChangesRolesAndAdminsRemoveOnlyMembers()
    {
        _settings.TeamsPerUser = 0;
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var otherAdmin = AddUser("otherAdmin");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { admin, otherAdmin, member })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }

        (await Controller(admin).SetRole(RoleChange(teamId, member, TeamRole.Admin))).Should().BeOfType<ForbidResult>();
        (await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin))).Should().BeOfType<NoContentResult>();
        (await Controller(owner).SetRole(RoleChange(teamId, otherAdmin, TeamRole.Admin))).Should().BeOfType<NoContentResult>();

        (await Controller(admin).RemoveMember(MemberRef(teamId, otherAdmin))).Should().BeOfType<ForbidResult>();
        (await Controller(admin).RemoveMember(MemberRef(teamId, owner))).Should().BeOfType<ForbidResult>();
        (await Controller(admin).RemoveMember(MemberRef(teamId, member))).Should().BeOfType<NoContentResult>();
        (await Controller(owner).RemoveMember(MemberRef(teamId, otherAdmin))).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Details(teamId)).Value!.Members.Select(m => m.PilotName).Should().Equal("pilot_owner", "pilot_admin");
    }

    [Fact]
    public async Task RevokeInvite_IsAllowedForAdminsOnly()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });

        (await Controller(invitee).RevokeInvite(MemberRef(teamId, invitee))).Should().BeOfType<ForbidResult>();
        (await Controller(owner).RevokeInvite(MemberRef(teamId, invitee))).Should().BeOfType<NoContentResult>();
        (await Controller(invitee).Overview()).Value!.Invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveUserFromTeams_HandsTheTeamToTheLongestServingAdminOrDeletesAnEmptyTeam()
    {
        _settings.TeamsPerUser = 0;
        var owner = AddUser("owner");
        var member = AddUser("member");
        var admin = AddUser("admin");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { member, admin })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }
        await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin));
        var lonely = AddUser("lonely");
        var lonelyTeamId = (await Controller(lonely).Create(new CreateTeamModel { Name = "Solo" })).Value!.Id;

        await Service().RemoveUserFromTeamsAsync(owner);
        await Service().RemoveUserFromTeamsAsync(lonely);

        var details = (await Controller(member).Details(teamId)).Value!;
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_admin" && m.Role == TeamRole.Owner);
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_member" && m.Role == TeamRole.Member);
        (await Controller(member).Details(lonelyTeamId)).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    private async Task<int> CreateTeamWithMembers(string name, int memberCount)
    {
        var owner = AddUser("owner of " + name);
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Name = name })).Value!.Id;

        for (var i = 1; i < memberCount; i++)
            _db.Set<TeamMember>().Add(new TeamMember { TeamId = teamId, UserId = AddUser($"{name} member {i}").Id });

        await _db.SaveChangesAsync();
        return teamId;
    }

    private static TeamMemberReferenceModel MemberRef(int teamId, ApplicationUser user) =>
        new() { TeamId = teamId, UserId = user.Id };

    private static SetMemberRoleModel RoleChange(int teamId, ApplicationUser user, TeamRole role) =>
        new() { TeamId = teamId, UserId = user.Id, Role = role };

    /// <summary>A controller acting as the given user, or as a visitor when there is none.</summary>
    private TeamsController Controller(ApplicationUser? user)
    {
        var controller = new TeamsController(
            _userManager,
            Service(),
            new Repository<Team>(_db),
            new Repository<TeamMember>(_db),
            new Repository<TeamInvite>(_db),
            new Repository<Pilot>(_db),
            Options.Create(_settings));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(user is null
                    ? new ClaimsIdentity()
                    : new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "test"))
            }
        };

        return controller;
    }
}
