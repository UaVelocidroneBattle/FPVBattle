using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Teams.Leaderboard;
using Veloci.Logic.Services;
using Veloci.Web.Controllers.Api.Teams;

namespace Veloci.Tests;

public class TeamCompetitionTests : TeamsTestBase
{
    private const string Cup = "open-class";
    private readonly FixedLeaderboardCalculator _liveLeaderboard = new();

    public TeamCompetitionTests()
    {
        _settings.MinTeamFinishers = 2;
    }

    [Fact]
    public async Task Roster_SkipsTeamsBelowMinPublicSizeAndDeletedTeams()
    {
        _settings.MinPublicTeamSize = 2;
        var (pairId, pair) = await CreateTeamAsync("Pair", 2);
        await CreateTeamAsync("Solo", 1);
        var (deletedId, deleted) = await CreateTeamAsync("Gone", 2);
        await Controller(deleted[0]).Remove(new TeamReferenceModel { TeamId = deletedId });

        var roster = await Roster().GetTeamIdsByPilotIdAsync();

        roster.Should().BeEquivalentTo(pair.ToDictionary(u => u.PilotId!.Value, _ => pairId));
    }

    [Fact]
    public async Task FinishedCompetition_StoresTheTeamLeaderboardAndShowsWhoFlew()
    {
        var (alphaId, alpha) = await CreateTeamAsync("Alpha", 2);
        var (bravoId, bravo) = await CreateTeamAsync("Bravo", 2);
        var competition = AddCompetition(CompetitionState.Closed, DateTime.Now,
            Result(alpha[0], 60_000), Result(alpha[1], 62_000), Result(bravo[0], 59_000), Result(bravo[1], 65_000));

        await FinishAsync(competition);

        var standings = await Service().GetDailyAsync(competition);

        standings.Select(s => (s.Team.Id, s.AverageTime, s.Rank, s.Points))
            .Should().Equal((alphaId, 61_000, 1, 100), (bravoId, 62_000, 2, 85));
        standings[0].Pilots.Select(p => p.TrackTime).Should().Equal(60_000, 62_000);
    }

    [Fact]
    public async Task RunningCompetition_IsRankedLiveWithoutStoringAnything()
    {
        var (alphaId, alpha) = await CreateTeamAsync("Alpha", 2);
        var competition = AddCompetition(CompetitionState.Started, DateTime.Now);
        _liveLeaderboard.Results = [Result(alpha[0], 60_000), Result(alpha[1], 61_000)];

        var standings = await Service().GetDailyAsync(competition);

        standings.Should().ContainSingle(s => s.Team.Id == alphaId && s.AverageTime == 60_500 && s.Rank == 1);
        _db.Set<TeamCompetitionResult>().Should().BeEmpty();
    }

    [Fact]
    public async Task Season_SumsTheMonthsPointsAndKeepsDeletedTeams()
    {
        var (alphaId, alpha) = await CreateTeamAsync("Alpha", 2);
        var (bravoId, bravo) = await CreateTeamAsync("Bravo", 2);
        var monthStart = new DateTime(2026, 11, 1);

        await FinishAsync(AddCompetition(CompetitionState.Closed, monthStart.AddDays(1),
            Result(alpha[0], 60_000), Result(alpha[1], 60_000), Result(bravo[0], 61_000), Result(bravo[1], 61_000)));
        await FinishAsync(AddCompetition(CompetitionState.Closed, monthStart.AddDays(2),
            Result(bravo[0], 61_000), Result(bravo[1], 61_000)));
        await FinishAsync(AddCompetition(CompetitionState.Closed, monthStart.AddDays(-1),
            Result(alpha[0], 60_000), Result(alpha[1], 60_000), Result(bravo[0], 61_000), Result(bravo[1], 61_000)));

        await Controller(bravo[0]).Remove(new TeamReferenceModel { TeamId = bravoId });
        _db.ChangeTracker.Clear();

        var season = await Service().GetSeasonAsync(Cup, monthStart, monthStart.AddMonths(1));

        season.Select(s => (s.Team.Id, s.Points, s.Rank, s.Team.IsDeleted))
            .Should().Equal((bravoId, 185, 1, true), (alphaId, 100, 2, false));
    }

    [Fact]
    public async Task DeletedTeam_IsHiddenAndFreesItsNameTagAndMembers()
    {
        var owner = AddUser("owner");
        var teamId = (await Controller(owner).Create(new CreateTeamModel { Tag = "ALF", Name = "Alpha" })).Value!.Id;

        await Controller(owner).Remove(new TeamReferenceModel { TeamId = teamId });
        _db.ChangeTracker.Clear();

        _db.Set<Team>().Should().BeEmpty();
        (await Controller(owner).Details(teamId)).Result.Should().BeOfType<NotFoundObjectResult>();
        (await Controller(owner).Create(new CreateTeamModel { Tag = "ALF", Name = "Alpha" })).Value.Should().NotBeNull();
    }

    private TeamLeaderboardService Service() => new(
        new Repository<Competition>(_db),
        new Repository<TeamCompetitionResult>(_db),
        new Repository<Team>(_db),
        _liveLeaderboard,
        Calculator(),
        Roster());

    private TeamLeaderboardCalculator Calculator() => new(new PointsCalculator(), Options.Create(_settings));

    private TeamRosterLookup Roster() => new(new Repository<TeamMember>(_db), Options.Create(_settings));

    private async Task FinishAsync(Competition competition)
    {
        competition.TeamCompetitionResults = await Service().RankFinishedAsync(competition);
        await _db.SaveChangesAsync();
    }

    private async Task<(int TeamId, List<ApplicationUser> Members)> CreateTeamAsync(string name, int size)
    {
        var members = Enumerable.Range(1, size).Select(i => AddUser($"{name}{i}")).ToList();
        var teamId = (await Controller(members[0]).Create(new CreateTeamModel { Tag = NextTag(), Name = name })).Value!.Id;

        foreach (var member in members.Skip(1))
            _db.Set<TeamMember>().Add(new TeamMember { TeamId = teamId, UserId = member.Id });

        await _db.SaveChangesAsync();
        return (teamId, members);
    }

    private Competition AddCompetition(CompetitionState state, DateTime startedOn, params CompetitionResults[] results)
    {
        var competition = new Competition
        {
            CupId = Cup,
            State = state,
            StartedOn = startedOn,
            Track = new Track { Name = "Track", Map = new TrackMap { Name = "Map" } },
            CompetitionResults = [.. results]
        };

        _db.Set<Competition>().Add(competition);
        _db.SaveChanges();
        return competition;
    }

    private CompetitionResults Result(ApplicationUser user, int trackTime) => new()
    {
        PilotId = user.PilotId!.Value,
        Pilot = _db.Set<Pilot>().Find(user.PilotId)!,
        TrackTime = trackTime
    };

    private class FixedLeaderboardCalculator : ILeaderboardCalculator
    {
        public List<CompetitionResults> Results { get; set; } = [];

        public List<CompetitionResults> GetLeaderboard(Competition competition) => Results;

        public List<LeagueLeaderboard> GetLeagueLeaderboard(Competition competition) => throw new NotSupportedException();

        public Task<List<LeagueSeasonLeaderboard>> GetSeasonLeaderboardAsync(string cupId, DateTime from, DateTime to) =>
            throw new NotSupportedException();
    }
}
