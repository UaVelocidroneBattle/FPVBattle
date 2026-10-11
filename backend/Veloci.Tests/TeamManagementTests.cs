using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams;
using Veloci.Web.Controllers.Admin.Teams;

namespace Veloci.Tests;

public class TeamManagementTests : TeamsTestBase
{
    [Fact]
    public async Task Index_ListsTeamsWithTheirOwnerAndCounts()
    {
        var team = await CreateTeamAsync("owner", "Alpha");
        await Service().AddMemberAsync(team, AddUser("member"));
        _settings.TeamsPerUser = 1;

        var result = await Controller().Index();

        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<TeamsViewModel>().Subject;
        model.Teams.Should().ContainSingle(t => t.Name == "Alpha" && t.OwnerPilotName == "pilot_owner" && t.MemberCount == 2);
    }

    [Fact]
    public async Task Index_OffersOnlyPilotsWhoCanOwnATeam()
    {
        await CreateTeamAsync("busy", "Busy team");
        AddUser("free");
        AddUser("nopilot", withPilot: false);

        var result = await Controller().Index();

        var model = (TeamsViewModel)result.Should().BeOfType<ViewResult>().Subject.Model!;
        model.OwnerCandidates.Should().Equal("pilot_free");
    }

    [Fact]
    public async Task Create_MakesThePickedPilotTheOwner()
    {
        AddUser("owner");
        var controller = Controller();

        var result = await controller.Create("Alpha", NextTag(), "First", "pilot_owner");

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(TeamManagementController.Details));
        controller.TempData["Success"].Should().NotBeNull();
        var details = await DetailsOf(controller, TeamId("Alpha"));
        details.Description.Should().Be("First");
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_owner" && m.Role == TeamRole.Owner);
    }

    [Fact]
    public async Task Create_ReportsAnUnknownOrUnregisteredPilotATakenNameAndTheLimits()
    {
        await CreateTeamAsync("busy", "Busy team");
        var controller = Controller();

        await controller.Create("Alpha", NextTag(), null, "nobody");
        controller.TempData["Error"].Should().Be("Pilot not found (names are case sensitive)");

        _db.Set<Pilot>().Add(new Pilot("unregistered") { Id = 100 });
        await _db.SaveChangesAsync();
        await controller.Create("Alpha", NextTag(), null, "unregistered");
        controller.TempData["Error"].Should().Be("This pilot is not registered yet");

        AddUser("free");
        await controller.Create("busy team", NextTag(), null, "pilot_free");
        controller.TempData["Error"].Should().Be("A team with this name already exists");

        await controller.Create("Second", NextTag(), null, "pilot_busy");
        controller.TempData["Error"].Should().Be("This pilot is already in the maximum number of teams");
    }

    [Fact]
    public async Task Edit_RenamesTheTeamAndRejectsATakenName()
    {
        var team = await CreateTeamAsync("owner", "Alpha");
        await CreateTeamAsync("other", "Taken");
        var controller = Controller();

        await controller.Edit(team.Id, "  Beta  ", "ZZ1", "New text");
        controller.TempData["Success"].Should().NotBeNull();

        await controller.Edit(team.Id, "taken", "ZZ1", null);
        controller.TempData["Error"].Should().Be("A team with this name already exists");

        var details = await DetailsOf(controller, team.Id);
        details.Name.Should().Be("Beta");
        details.Description.Should().Be("New text");
    }

    [Fact]
    public async Task Edit_ChangesTheTagAndRejectsInvalidTakenAndBlacklistedOnes()
    {
        var team = await CreateTeamAsync("owner", "Alpha");
        var taken = (await CreateTeamAsync("other", "Beta")).Tag;
        _settings.BlacklistedTags = ["ADMIN"];
        var controller = Controller();

        await controller.Edit(team.Id, "Alpha", "na1", null);
        controller.TempData["Success"].Should().NotBeNull();
        (await DetailsOf(controller, team.Id)).Tag.Should().Be("NA1");

        await controller.Edit(team.Id, "Alpha", "A", null);
        controller.TempData["Error"].Should().Be(TeamMessages.InvalidTag);

        await controller.Edit(team.Id, "Alpha", taken!.ToLowerInvariant(), null);
        controller.TempData["Error"].Should().Be(TeamMessages.TagTaken);

        await controller.Edit(team.Id, "Alpha", "Admin", null);
        controller.TempData["Error"].Should().Be(TeamMessages.TagNotAllowed);

        (await DetailsOf(controller, team.Id)).Tag.Should().Be("NA1");
    }

    [Fact]
    public async Task Delete_RemovesTheTeamWithItsMembers()
    {
        var team = await CreateTeamAsync("owner", "Alpha");

        var result = await Controller().Delete(team.Id);

        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(TeamManagementController.Index));
        _db.Set<Team>().Should().BeEmpty();
        _db.Set<TeamMember>().Should().BeEmpty();
    }

    [Fact]
    public async Task AddMember_AddsThePilotAndDropsTheirInvite()
    {
        _settings.TeamsPerUser = 0;
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var team = (await Service().CreateAsync(owner, "Alpha", NextTag(), null)).Team!;
        await Service().InviteAsync(owner, team, invitee);
        var controller = Controller();

        await controller.AddMember(team.Id, PilotNameOf(invitee));

        controller.TempData["Success"].Should().NotBeNull();
        var details = await DetailsOf(controller, team.Id);
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_invitee" && m.Role == TeamRole.Member);
        details.Invites.Should().BeEmpty();
    }

    [Fact]
    public async Task Details_OffersOnlyPilotsWhoCanJoinTheTeam()
    {
        var team = await CreateTeamAsync("owner", "Alpha");
        await CreateTeamAsync("busy", "Busy team");
        AddUser("free");
        AddUser("nopilot", withPilot: false);
        var controller = Controller();

        (await DetailsOf(controller, team.Id)).MemberCandidates.Should().Equal("pilot_free");

        _settings.MaxTeamSize = 1;
        (await DetailsOf(controller, team.Id)).MemberCandidates.Should().BeEmpty();
    }

    [Fact]
    public async Task AddMember_RespectsTheLimitsAndRejectsDuplicates()
    {
        var team = await CreateTeamAsync("owner", "Alpha");
        await CreateTeamAsync("busy", "Busy team");
        var controller = Controller();

        await controller.AddMember(team.Id, "pilot_busy");
        controller.TempData["Error"].Should().Be("This pilot is already in the maximum number of teams");

        await controller.AddMember(team.Id, "pilot_owner");
        controller.TempData["Error"].Should().Be("This pilot is already a member of the team");

        _settings.TeamsPerUser = 0;
        _settings.MaxTeamSize = 1;
        AddUser("late");
        await controller.AddMember(team.Id, "pilot_late");
        controller.TempData["Error"].Should().Be("The team is full");
    }

    [Fact]
    public async Task SetRole_ChangesTheRoleOfAMemberButNeverTheOwners()
    {
        _settings.TeamsPerUser = 0;
        var team = await CreateTeamAsync("owner", "Alpha");
        var member = AddUser("member");
        await Service().AddMemberAsync(team, member);
        var controller = Controller();

        await controller.SetRole(team.Id, member.Id, TeamRole.Admin);
        (await DetailsOf(controller, team.Id)).Members.Should().ContainSingle(m => m.UserId == member.Id && m.Role == TeamRole.Admin);

        await controller.SetRole(team.Id, "owner", TeamRole.Member);
        controller.TempData["Error"].Should().NotBeNull();
        await controller.SetRole(team.Id, member.Id, TeamRole.Owner);
        (await DetailsOf(controller, team.Id)).Members.Should().ContainSingle(m => m.Role == TeamRole.Owner && m.UserId == "owner");
    }

    [Fact]
    public async Task RemoveMember_RemovesAMemberButNotTheOwner()
    {
        _settings.TeamsPerUser = 0;
        var team = await CreateTeamAsync("owner", "Alpha");
        var member = AddUser("member");
        await Service().AddMemberAsync(team, member);
        var controller = Controller();

        await controller.RemoveMember(team.Id, member.Id);
        await controller.RemoveMember(team.Id, "owner");

        controller.TempData["Error"].Should().NotBeNull();
        (await DetailsOf(controller, team.Id)).Members.Should().ContainSingle(m => m.UserId == "owner");
    }

    [Fact]
    public async Task RevokeInvite_RemovesThePendingInvite()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var team = (await Service().CreateAsync(owner, "Alpha", NextTag(), null)).Team!;
        await Service().InviteAsync(owner, team, invitee);
        var controller = Controller();

        await controller.RevokeInvite(team.Id, invitee.Id);

        (await DetailsOf(controller, team.Id)).Invites.Should().BeEmpty();
    }

    private async Task<Team> CreateTeamAsync(string ownerName, string teamName)
    {
        var owner = AddUser(ownerName);
        var team = (await Service().CreateAsync(owner, teamName, NextTag(), null)).Team!;
        _settings.TeamsPerUser = 1;
        return team;
    }

    private int TeamId(string name) => _db.Set<Team>().Single(t => t.Name == name).Id;

    private static async Task<TeamDetailsViewModel> DetailsOf(TeamManagementController controller, int teamId)
    {
        var result = await controller.Details(teamId);
        return (TeamDetailsViewModel)result.Should().BeOfType<ViewResult>().Subject.Model!;
    }

    private TeamManagementController Controller() => new(
        new Repository<Team>(_db),
        new Repository<TeamMember>(_db),
        new Repository<TeamInvite>(_db),
        new Repository<ApplicationUser>(_db),
        new Repository<Pilot>(_db),
        Service(),
        Options.Create(_settings))
    {
        TempData = new TempDataDictionary(new DefaultHttpContext(), new InMemoryTempDataProvider())
    };

    private class InMemoryTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
