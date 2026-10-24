using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Notifications.Channels;
using Veloci.Logic.Features.Notifications.Services;
using Veloci.Logic.Features.Teams.NotificationHandlers;
using Veloci.Logic.Features.Teams.Notifications;
using Veloci.Web.Controllers.Api.Teams;

namespace Veloci.Tests;

public class TeamJoinRequestsTests : TeamsTestBase
{
    [Fact]
    public async Task Request_ThenApprove_MakesThePilotAMemberAndDropsTheirOtherInvites()
    {
        var owner = AddUser("owner");
        var ownerA = AddUser("ownerA");
        var pilot = AddUser("pilot");
        var teamB = await CreateTeamAsync(owner, "Team B");
        var teamA = await CreateTeamAsync(ownerA, "Team A");
        await Controller(ownerA).Invite(new InviteModel { TeamId = teamA, PilotName = PilotNameOf(pilot) });

        (await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teamB })).Should().BeOfType<NoContentResult>();

        (await Controller(pilot).Overview()).Value!.JoinRequests.Should().ContainSingle(r => r.TeamId == teamB);
        (await Controller(pilot).Details(teamB)).Value!.MyJoinRequestPending.Should().BeTrue();
        var ownersView = (await Controller(owner).Details(teamB)).Value!;
        ownersView.JoinRequests.Should().ContainSingle(r => r.PilotName == "pilot_pilot");
        ownersView.PendingInvites.Should().BeEmpty();
        _publisher.Published.OfType<TeamJoinRequestedNotification>().Should().ContainSingle();

        (await Controller(owner).ApproveJoinRequest(MemberRef(teamB, pilot))).Should().BeOfType<NoContentResult>();

        (await Controller(pilot).Details(teamB)).Value!.MyRole.Should().Be(TeamRole.Member);
        (await Controller(pilot).Overview()).Value!.Invitations.Should().BeEmpty();
        _db.Set<TeamInvite>().Should().BeEmpty();
        _publisher.Published.OfType<TeamJoinRequestApprovedNotification>().Should().ContainSingle(e => e.Member.Id == pilot.Id);
    }

    [Fact]
    public async Task Requester_CannotAcceptTheirOwnRequest()
    {
        var teamId = await CreateTeamAsync(AddUser("owner"), "Alpha");
        var pilot = AddUser("pilot");
        await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teamId });

        (await Controller(pilot).Accept(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<NotFoundObjectResult>();
        (await Controller(pilot).Overview()).Value!.Invitations.Should().BeEmpty();
        (await Controller(pilot).Details(teamId)).Value!.MyRole.Should().BeNull();
    }

    [Fact]
    public async Task OnlyOwnersAndAdminsReviewRequests()
    {
        var owner = AddUser("owner");
        var member = AddUser("member");
        var pilot = AddUser("pilot");
        var teamId = await CreateTeamAsync(owner, "Alpha");
        await Service().AddMemberAsync(_db.Set<Team>().Find(teamId)!, member);
        await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teamId });

        (await Controller(member).Details(teamId)).Value!.JoinRequests.Should().BeEmpty();
        (await Controller(member).ApproveJoinRequest(MemberRef(teamId, pilot))).Should().BeOfType<ForbidResult>();
        (await Controller(member).RejectJoinRequest(MemberRef(teamId, pilot))).Should().BeOfType<ForbidResult>();

        (await Controller(owner).RejectJoinRequest(MemberRef(teamId, pilot))).Should().BeOfType<NoContentResult>();
        (await Controller(pilot).Overview()).Value!.JoinRequests.Should().BeEmpty();
    }

    [Fact]
    public async Task Request_IsLimitedToTheConfiguredNumberOfPendingRequests()
    {
        _settings.MaxPendingJoinRequests = 2;
        var pilot = AddUser("pilot");
        var teams = new List<int>();
        for (var i = 1; i <= 3; i++)
            teams.Add(await CreateTeamAsync(AddUser($"owner{i}"), $"Team {i}"));

        await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teams[0] });
        await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teams[1] });

        (await Controller(pilot).Details(teams[2])).Value!.JoinRequestBlockedReason.Should().Contain("up to 2");
        (await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teams[2] })).Should().BeOfType<ConflictObjectResult>();

        await Controller(pilot).CancelJoinRequest(new TeamReferenceModel { TeamId = teams[0] });
        (await Controller(pilot).RequestToJoin(new TeamReferenceModel { TeamId = teams[2] })).Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Request_IsRefusedForATeamThatIsNotPublicOrWhenAlreadyInATeam()
    {
        _settings.MinPublicTeamSize = 2;
        var owner = AddUser("owner");
        var tinyTeam = await CreateTeamAsync(owner, "Tiny");

        (await Controller(AddUser("pilot")).RequestToJoin(new TeamReferenceModel { TeamId = tinyTeam })).Should().BeOfType<NotFoundObjectResult>();
        (await Controller(owner).RequestToJoin(new TeamReferenceModel { TeamId = tinyTeam })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(AddUser("nopilot", withPilot: false)).RequestToJoin(new TeamReferenceModel { TeamId = tinyTeam }))
            .Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task InvitingAPilotWhoAsked_LetsThemIn()
    {
        var owner = AddUser("owner");
        var asker = AddUser("asker");
        var teamId = await CreateTeamAsync(owner, "Alpha");

        await Controller(asker).RequestToJoin(new TeamReferenceModel { TeamId = teamId });
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(asker) });

        (await Controller(owner).Details(teamId)).Value!.Members.Select(m => m.PilotName)
            .Should().BeEquivalentTo("pilot_owner", "pilot_asker");
    }

    [Fact]
    public async Task InvitedPilot_AnswersTheInviteInsteadOfAsking()
    {
        var owner = AddUser("owner");
        var invited = AddUser("invited");
        var teamId = await CreateTeamAsync(owner, "Alpha");
        await Controller(owner).Invite(new InviteModel { TeamId = teamId, PilotName = PilotNameOf(invited) });

        (await Controller(invited).Details(teamId)).Value!.JoinRequestBlockedReason.Should().Contain("Accept or decline");
        (await Controller(invited).RequestToJoin(new TeamReferenceModel { TeamId = teamId })).Should().BeOfType<ConflictObjectResult>();
        (await Controller(invited).Overview()).Value!.Invitations.Should().ContainSingle();
    }

    [Fact]
    public async Task InAppTeamJoinRequestHandler_TellsTheOwnerAndAdminsAndThenThePilot()
    {
        var owner = AddUser("owner");
        var admin = AddUser("admin");
        var member = AddUser("member");
        var pilot = AddUser("pilot");
        var teamId = await CreateTeamAsync(owner, "Alpha");
        var team = _db.Set<Team>().Find(teamId)!;
        await Service().AddMemberAsync(team, admin);
        await Service().AddMemberAsync(team, member);
        await Service().SetRoleAsync(_db.Set<TeamMember>().Single(m => m.UserId == admin.Id), TeamRole.Admin);
        var handler = new InAppTeamJoinRequestHandler(
            new UserNotificationService([new InAppNotificationChannel(new Repository<UserNotification>(_db))], new Repository<ApplicationUser>(_db)),
            new Repository<TeamMember>(_db));

        await handler.Handle(new TeamJoinRequestedNotification(team, pilot), CancellationToken.None);
        await handler.Handle(new TeamJoinRequestApprovedNotification(team, pilot), CancellationToken.None);

        var notifications = _db.Set<UserNotification>().ToList();
        notifications.Where(n => n.Type == NotificationType.TeamJoinRequest).Select(n => n.UserId)
            .Should().BeEquivalentTo(owner.Id, admin.Id);
        notifications.Should().Contain(n => n.Text == "pilot_pilot asked to join your team Alpha");
        notifications.Should().ContainSingle(n => n.Type == NotificationType.TeamJoinRequestApproved && n.UserId == pilot.Id);
    }

    private async Task<int> CreateTeamAsync(ApplicationUser owner, string name)
    {
        return (await Service().CreateAsync(owner, name, NextTag(), null)).Team!.Id;
    }
}
