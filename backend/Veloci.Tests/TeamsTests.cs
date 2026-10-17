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
using Veloci.Logic.Features.Notifications.Channels;
using Veloci.Logic.Features.Notifications.Services;
using Veloci.Logic.Features.Teams.NotificationHandlers;
using Veloci.Logic.Features.Teams.Notifications;
using Veloci.Logic.Features.Teams.Services;
using Veloci.Web.Controllers.Api.Teams;

namespace Veloci.Tests;

public class TeamsTests : TeamsTestBase
{
    [Fact]
    public async Task Create_MakesTheCreatorTheOwner()
    {
        var owner = AddUser("owner");

        var created = await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha", Description = "First" });

        created.Value!.Name.Should().Be("Alpha");
        var overview = (await Controller(owner).Overview()).Value!;
        overview.MyTeam.Should().BeEquivalentTo(new { Name = "Alpha", MemberCount = 1 });
        (await Controller(owner).Details(created.Value!.Id)).Value!.MyRole.Should().Be(TeamRole.Owner);
    }

    [Fact]
    public async Task Create_RejectsDuplicateNamesIgnoringCase()
    {
        await Controller(AddUser("first")).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" });

        var duplicate = await Controller(AddUser("second")).Create(new CreateTeamModel { Tag = NextTag(), Name = "aLPHA" });

        duplicate.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_StoresTheTagInUpperCase()
    {
        var created = await Controller(AddUser("owner")).Create(new CreateTeamModel { Tag = " na1 ", Name = "Alpha" });

        created.Value!.Tag.Should().Be("NA1");
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("ABCDEF")]
    [InlineData("A B")]
    [InlineData("AB-")]
    [InlineData("ДРОН")]
    public async Task Create_RejectsATagOfTheWrongLengthOrWithOtherSymbols(string tag)
    {
        var result = await Controller(AddUser("owner")).Create(new CreateTeamModel { Tag = tag, Name = "Alpha" });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_RejectsATakenTagIgnoringCase()
    {
        await Controller(AddUser("first")).Create(new CreateTeamModel { Tag = "NAVI", Name = "Alpha" });

        var duplicate = await Controller(AddUser("second")).Create(new CreateTeamModel { Tag = "nAvI", Name = "Beta" });

        duplicate.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Update_LetsOwnersAndAdminsChangeTheTagButNotMembers()
    {
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = "OLD", Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { admin, member })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }
        await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin));

        (await Controller(member).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "NEW" })).Should().BeOfType<ForbidResult>();
        (await Controller(admin).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "mid" })).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "new" })).Should().BeOfType<NoContentResult>();

        (await Controller(owner).Details(teamId)).Value!.Tag.Should().Be("NEW");
    }

    [Fact]
    public async Task Update_RejectsATakenInvalidOrBlacklistedTagButKeepsTheOwnTagInAnotherCase()
    {
        var owner = AddUser("owner");
        await Controller(AddUser("other")).Create(new CreateTeamModel { Tag = "NAVI", Name = "Beta" });
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = "ABC", Name = "Alpha" })).Value!.Id;
        _settings.BlacklistedTags = ["ABC", "ADMIN"];

        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "navi" })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "A" })).Should().BeOfType<BadRequestObjectResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha", Tag = "admin" })).Should().BeOfType<BadRequestObjectResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Name = "Alpha 2", Tag = "abc" })).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Create_RejectsABlacklistedTagIgnoringCase()
    {
        _settings.BlacklistedTags = ["Admin"];

        var result = await Controller(AddUser("owner")).Create(new CreateTeamModel { Tag = "admin", Name = "Alpha" });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_NeedsEnoughRaceDaysOrAPayingPatron()
    {
        _settings.MinRaceDaysToCreate = 10;
        _settings.AllowPatronsToCreate = true;
        var rookie = AddUser("rookie");
        var veteran = AddUser("veteran");
        var patron = AddUser("patron");
        var lapsedPatron = AddUser("lapsed");
        _db.Set<Pilot>().Find(veteran.PilotId)!.TotalRaceDays = 10;
        AddSupporter(patron, PatreonSupporter.ActivePatronStatus);
        AddSupporter(lapsedPatron, "former_patron");

        (await Controller(rookie).Overview()).Value!.CreateBlockedReason.Should().Contain("10 race days");
        (await Controller(rookie).Create(new CreateTeamModel { Tag = NextTag(), Name = "Rookies" })).Result.Should().BeOfType<BadRequestObjectResult>();
        (await Controller(lapsedPatron).Create(new CreateTeamModel { Tag = NextTag(), Name = "Lapsed" })).Result.Should().BeOfType<BadRequestObjectResult>();
        (await Controller(veteran).Create(new CreateTeamModel { Tag = NextTag(), Name = "Veterans" })).Value.Should().NotBeNull();
        (await Controller(patron).Overview()).Value!.CreateBlockedReason.Should().BeNull();
        (await Controller(patron).Create(new CreateTeamModel { Tag = NextTag(), Name = "Patrons" })).Value.Should().NotBeNull();

        _settings.AllowPatronsToCreate = false;
        (await Controller(AddUser("patron2")).Overview()).Value!.CreateBlockedReason.Should().NotContain("Patreon");
    }

    [Fact]
    public async Task Create_RequiresALinkedPilot()
    {
        var result = await Controller(AddUser("nopilot", withPilot: false)).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_LetsOwnersAndAdminsEditTheTeamButNotMembers()
    {
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { admin, member })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }
        await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin));

        (await Controller(member).Update(new UpdateTeamModel { TeamId = teamId, Tag = "ZZ1", Name = "Hacked" })).Should().BeOfType<ForbidResult>();
        (await Controller(admin).Update(new UpdateTeamModel { TeamId = teamId, Tag = "ZZ1", Name = "  Beta  ", Description = "New text" }))
            .Should().BeOfType<NoContentResult>();

        var details = (await Controller(owner).Details(teamId)).Value!;
        details.Name.Should().Be("Beta");
        details.Description.Should().Be("New text");
    }

    [Fact]
    public async Task Update_RejectsATakenNameButAllowsChangingTheCaseOfTheOwnName()
    {
        var owner = AddUser("owner");
        await Controller(AddUser("other")).Create(new CreateTeamModel { Tag = NextTag(), Name = "Taken" });
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;

        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Tag = "ZZ1", Name = "tAKEN" })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Tag = "ZZ1", Name = "ALPHA" })).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Update(new UpdateTeamModel { TeamId = teamId, Tag = "ZZ1", Name = "   " })).Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Invite_ThenAccept_MakesTheInviteeAMember()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;

        var invited = await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });
        invited.Should().BeOfType<OkResult>();
        _publisher.Published.Should().ContainSingle(e => e is TeamInviteSentNotification && ((TeamInviteSentNotification)e).Invitee.Id == invitee.Id);

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
    public async Task InAppTeamInviteHandler_PutsTheInviteInTheInviteesBell()
    {
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var team = (await Service().CreateAsync(owner, "Alpha", NextTag(), null)).Team!;
        var notifications = new UserNotificationService(
            [new InAppNotificationChannel(new Repository<UserNotification>(_db))], new Repository<ApplicationUser>(_db));

        await new InAppTeamInviteHandler(notifications).Handle(new TeamInviteSentNotification(team, owner, invitee), CancellationToken.None);

        _db.Set<UserNotification>().Should().ContainSingle(n =>
            n.UserId == invitee.Id && n.Type == NotificationType.TeamInvite && n.Text == "pilot_owner invited you to join the team Alpha");
    }

    [Fact]
    public async Task Invite_IsRejectedWhenTheInviteeIsAlreadyInATeam()
    {
        var owner = AddUser("owner");
        var busy = AddUser("busy");
        await Controller(busy).Create(new CreateTeamModel { Tag = NextTag(), Name = "Busy team" });
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;

        var result = await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(busy) });

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Accept_IsRejectedWhenTheUserIsAlreadyInATeam()
    {
        var ownerA = AddUser("ownerA");
        var ownerB = AddUser("ownerB");
        var invitee = AddUser("invitee");
        var teamA = (await Controller(ownerA).Create(new CreateTeamModel { Tag = NextTag(), Name = "A" })).Value!.Id;
        var teamB = (await Controller(ownerB).Create(new CreateTeamModel { Tag = NextTag(), Name = "B" })).Value!.Id;
        await Controller(ownerA).Invite(new InviteModel { TeamId = teamA, PilotName = PilotNameOf(invitee) });
        await Controller(ownerB).Invite(new InviteModel { TeamId = teamB, PilotName = PilotNameOf(invitee) });

        await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamA });
        var second = await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamB });

        second.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Database_KeepsAUserInOneTeamOnly()
    {
        var owner = AddUser("owner");
        await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" });
        var otherTeamId = (await Controller(AddUser("other")).Create(new CreateTeamModel { Tag = NextTag(), Name = "Beta" })).Value!.Id;

        _db.Set<TeamMember>().Add(new TeamMember { TeamId = otherTeamId, UserId = owner.Id });
        var secondMembership = () => _db.SaveChangesAsync();

        await secondMembership.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Search_ExcludesMyTeamAndOrdersBySizeThenFiltersByName()
    {
        var me = AddUser("me");
        await Controller(me).Create(new CreateTeamModel { Tag = NextTag(), Name = "Mine" });
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
    public async Task TeamBelowMinPublicSize_IsHiddenFromEveryoneButMembersAndInvitees()
    {
        _settings.MinPublicTeamSize = 3;
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var stranger = AddUser("stranger");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Tiny" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });
        await CreateTeamWithMembers("Crowd", 3);

        (await Controller(null).Search(null)).Value!.Select(t => t.Name).Should().Equal("Crowd");
        (await Controller(stranger).Search("Tiny")).Value.Should().BeEmpty();
        (await Controller(null).Details(teamId)).Result.Should().BeOfType<NotFoundObjectResult>();
        (await Controller(stranger).Details(teamId)).Result.Should().BeOfType<NotFoundObjectResult>();
        (await Controller(owner).Details(teamId)).Value.Should().NotBeNull();
        (await Controller(invitee).Details(teamId)).Value.Should().NotBeNull();

        await Controller(invitee).Accept(new TeamReferenceModel { TeamId = teamId });
        var third = AddUser("third");
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(third) });
        await Controller(third).Accept(new TeamReferenceModel { TeamId = teamId });

        (await Controller(null).Details(teamId)).Value.Should().NotBeNull();
    }

    [Fact]
    public async Task TeamTagLookup_SkipsTeamsBelowMinPublicSize()
    {
        _settings.MinPublicTeamSize = 3;
        var owner = AddUser("owner");
        await Controller(owner).Create(new CreateTeamModel { Tag = "TNY", Name = "Tiny" });
        var lookup = new TeamTagLookup(new Repository<TeamMember>(_db), Options.Create(_settings));

        (await lookup.GetTagsByPilotNameAsync()).Should().BeEmpty();

        _settings.MinPublicTeamSize = 1;

        (await lookup.GetTagsByPilotNameAsync()).Should().ContainKey(PilotNameOf(owner)).WhoseValue.Should().Be("TNY");
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
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invited) });
        await Controller(busy).Create(new CreateTeamModel { Tag = NextTag(), Name = "Busy team" });

        var all = (await Controller(owner).InviteCandidates(teamId, "PILOT_")).Value!;

        all.Select(p => p.Name).Should().Equal("pilot_free");
    }

    [Fact]
    public async Task InviteCandidates_NeedsAtLeastTwoCharactersAndAnOwnerOrAdmin()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
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
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha", Description = "Text" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });
        var visitor = Controller(null);

        var overview = (await visitor.Overview()).Value!;
        overview.MyTeam.Should().BeNull();
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
        var owner = AddUser("owner");
        var invitee = AddUser("invitee");
        var other = AddUser("other");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
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
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
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
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(member) });
        await Controller(member).Accept(new TeamReferenceModel { TeamId = teamId });

        (await Controller(owner).Leave(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(member).Leave(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<NoContentResult>();
        (await Controller(owner).Details(teamId)).Value!.Members.Should().ContainSingle();
    }

    [Fact]
    public async Task Roles_OnlyTheOwnerChangesRolesAndAdminsRemoveOnlyMembers()
    {
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var otherAdmin = AddUser("otherAdmin");
        var member = AddUser("member");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
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
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invitee) });

        (await Controller(invitee).RevokeInvite(MemberRef(teamId, invitee))).Should().BeOfType<ForbidResult>();
        (await Controller(owner).RevokeInvite(MemberRef(teamId, invitee))).Should().BeOfType<NoContentResult>();
        (await Controller(invitee).Overview()).Value!.Invitations.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveUserFromTeam_HandsTheTeamToTheLongestServingAdminOrDeletesAnEmptyTeam()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var admin = AddUser("admin");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = "Alpha" })).Value!.Id;
        foreach (var user in new[] { member, admin })
        {
            await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(user) });
            await Controller(user).Accept(new TeamReferenceModel { TeamId = teamId });
        }
        await Controller(owner).SetRole(RoleChange(teamId, admin, TeamRole.Admin));
        var lonely = AddUser("lonely");
        var lonelyTeamId = (await Controller(lonely).Create(new CreateTeamModel { Tag = NextTag(), Name = "Solo" })).Value!.Id;

        await Service().RemoveUserFromTeamAsync(owner);
        await Service().RemoveUserFromTeamAsync(lonely);

        var details = (await Controller(member).Details(teamId)).Value!;
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_admin" && m.Role == TeamRole.Owner);
        details.Members.Should().ContainSingle(m => m.PilotName == "pilot_member" && m.Role == TeamRole.Member);
        (await Controller(member).Details(lonelyTeamId)).Result.Should().BeOfType<NotFoundObjectResult>();
    }

    private async Task<int> CreateTeamWithMembers(string name, int memberCount)
    {
        var owner = AddUser("owner of " + name);
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = NextTag(), Name = name })).Value!.Id;

        for (var i = 1; i < memberCount; i++)
            _db.Set<TeamMember>().Add(new TeamMember { TeamId = teamId, UserId = AddUser($"{name} member {i}").Id });

        await _db.SaveChangesAsync();
        return teamId;
    }

    private void AddSupporter(ApplicationUser user, string status)
    {
        _db.Set<PatreonSupporter>().Add(new PatreonSupporter
        {
            PatreonId = user.Id, Name = user.Id, Status = status, Amount = 5, PilotId = user.PilotId
        });
        _db.SaveChanges();
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
            new TeamCreationPolicy(
                new Repository<Pilot>(_db), new Repository<TeamMember>(_db), new Repository<PatreonSupporter>(_db), Options.Create(_settings)),
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
