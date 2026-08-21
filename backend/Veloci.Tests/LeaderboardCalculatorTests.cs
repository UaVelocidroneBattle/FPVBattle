using FluentAssertions;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Features.Leagues;
using Veloci.Logic.Services;

namespace Veloci.Tests;

public class LeaderboardCalculatorTests
{
    private readonly LeaderboardCalculator _calculator;

    public LeaderboardCalculatorTests()
    {
        _calculator = new LeaderboardCalculator(
            null!,
            new LeaguesDisabledCupService(),
            new PointsCalculator(),
            Options.Create(new PaceRatingSettings()));
    }

    [Fact]
    public void GetLeaderboard_WithQuadOfTheDay_ShouldScorePointsByRankAmongQuadOfTheDayPilotsOnly()
    {
        // Arrange: pilot "Fastest" wins on time but didn't fly the quad of the day, so a QOD pilot
        // finishing behind them should still get the full 100 points, not 85.
        var quadOfTheDay = new QuadModel { Id = 1, Name = "iFlight Nazgul5", Class = QuadClasses.Race };

        var competition = new Competition
        {
            QuadOfTheDay = quadOfTheDay,
            TimeDeltas =
            [
                CreateDelta(pilotId: 1, name: "Fastest", trackTime: 60_000, modelName: "Some Other Quad", rank: 1),
                CreateDelta(pilotId: 2, name: "QodWinner", trackTime: 61_000, modelName: quadOfTheDay.Name, rank: 2),
                CreateDelta(pilotId: 3, name: "QodRunnerUp", trackTime: 62_000, modelName: quadOfTheDay.Name, rank: 3)
            ]
        };

        // Act
        var leaderboard = _calculator.GetLeaderboard(competition);

        // Assert
        leaderboard.Single(r => r.PilotId == 1).Points.Should().Be(1);
        leaderboard.Single(r => r.PilotId == 2).Points.Should().Be(100);
        leaderboard.Single(r => r.PilotId == 3).Points.Should().Be(85);
    }

    [Fact]
    public void GetLeaderboard_WithQuadOfTheDay_ShouldKeepLocalRankBasedOnAbsoluteFinishTime()
    {
        var quadOfTheDay = new QuadModel { Id = 1, Name = "iFlight Nazgul5", Class = QuadClasses.Race };

        var competition = new Competition
        {
            QuadOfTheDay = quadOfTheDay,
            TimeDeltas =
            [
                CreateDelta(pilotId: 1, name: "Fastest", trackTime: 60_000, modelName: "Some Other Quad", rank: 1),
                CreateDelta(pilotId: 2, name: "QodWinner", trackTime: 61_000, modelName: quadOfTheDay.Name, rank: 2)
            ]
        };

        var leaderboard = _calculator.GetLeaderboard(competition);

        leaderboard.Single(r => r.PilotId == 1).LocalRank.Should().Be(1);
        leaderboard.Single(r => r.PilotId == 2).LocalRank.Should().Be(2);
    }

    [Fact]
    public void GetLeaderboard_WithoutQuadOfTheDay_ShouldScorePointsByAbsolutePosition()
    {
        var competition = new Competition
        {
            QuadOfTheDay = null,
            TimeDeltas =
            [
                CreateDelta(pilotId: 1, name: "First", trackTime: 60_000, modelName: "Any Quad", rank: 1),
                CreateDelta(pilotId: 2, name: "Second", trackTime: 61_000, modelName: "Any Quad", rank: 2),
                CreateDelta(pilotId: 3, name: "Third", trackTime: 62_000, modelName: "Any Quad", rank: 3)
            ]
        };

        var leaderboard = _calculator.GetLeaderboard(competition);

        leaderboard.Single(r => r.PilotId == 1).Points.Should().Be(100);
        leaderboard.Single(r => r.PilotId == 2).Points.Should().Be(85);
        leaderboard.Single(r => r.PilotId == 3).Points.Should().Be(75);
    }

    [Fact]
    public void GetLeagueLeaderboard_WithFewerThanThreePilots_ShouldLeaveGapToLeaderNull()
    {
        var competition = new Competition
        {
            CupId = "test-cup",
            QuadOfTheDay = null,
            TimeDeltas =
            [
                CreateDelta(pilotId: 1, name: "First", trackTime: 60_000, modelName: "Any Quad", rank: 1),
                CreateDelta(pilotId: 2, name: "Second", trackTime: 62_000, modelName: "Any Quad", rank: 2)
            ]
        };

        var results = _calculator.GetLeagueLeaderboard(competition).SelectMany(l => l.Results);

        results.Should().OnlyContain(r => r.GapToLeaderPercent == null);
    }

    [Fact]
    public void GetLeagueLeaderboard_WithThreeOrMorePilots_ShouldSetGapToLeaderRelativeToTopThreeAverage()
    {
        var competition = new Competition
        {
            CupId = "test-cup",
            QuadOfTheDay = null,
            TimeDeltas =
            [
                CreateDelta(pilotId: 1, name: "First", trackTime: 60_000, modelName: "Any Quad", rank: 1),
                CreateDelta(pilotId: 2, name: "Second", trackTime: 61_000, modelName: "Any Quad", rank: 2),
                CreateDelta(pilotId: 3, name: "Third", trackTime: 62_000, modelName: "Any Quad", rank: 3),
                CreateDelta(pilotId: 4, name: "Fourth", trackTime: 66_000, modelName: "Any Quad", rank: 4)
            ]
        };

        // Reference = average(60_000, 61_000, 62_000) = 61_000
        var results = _calculator.GetLeagueLeaderboard(competition).SelectMany(l => l.Results).ToList();

        results.Single(r => r.PilotId == 1).GapToLeaderPercent.Should().BeApproximately(-1.639, 0.001);
        results.Single(r => r.PilotId == 2).GapToLeaderPercent.Should().BeApproximately(0, 0.001);
        results.Single(r => r.PilotId == 3).GapToLeaderPercent.Should().BeApproximately(1.639, 0.001);
        results.Single(r => r.PilotId == 4).GapToLeaderPercent.Should().BeApproximately(8.197, 0.001);
    }

    [Fact]
    public void GetLeagueLeaderboard_WithResultsAlreadyPersisted_ShouldStillComputeGapToLeaderFromStoredTrackTimes()
    {
        // Simulates a closed competition read back from the database: CompetitionResults is already
        // populated (TimeDeltas typically isn't loaded at all), and GapToLeaderPercent comes back at its
        // CLR default because it's Ignore()d in ApplicationDbContext and was never persisted.
        var competition = new Competition
        {
            CupId = "test-cup",
            CompetitionResults =
            [
                CreateResult(pilotId: 1, name: "First", trackTime: 60_000),
                CreateResult(pilotId: 2, name: "Second", trackTime: 61_000),
                CreateResult(pilotId: 3, name: "Third", trackTime: 62_000)
            ]
        };

        var results = _calculator.GetLeagueLeaderboard(competition).SelectMany(l => l.Results).ToList();

        results.Single(r => r.PilotId == 1).GapToLeaderPercent.Should().BeApproximately(-1.639, 0.001);
        results.Single(r => r.PilotId == 3).GapToLeaderPercent.Should().BeApproximately(1.639, 0.001);
    }

    private static CompetitionResults CreateResult(int pilotId, string name, int trackTime)
    {
        return new CompetitionResults
        {
            CompetitionId = "test-competition",
            PilotId = pilotId,
            Pilot = new Pilot(name) { Id = pilotId },
            TrackTime = trackTime
        };
    }

    private static TrackTimeDelta CreateDelta(int pilotId, string name, int trackTime, string modelName, int rank)
    {
        return new TrackTimeDelta
        {
            CompetitionId = "test-competition",
            PilotId = pilotId,
            Pilot = new Pilot(name) { Id = pilotId },
            TrackTime = trackTime,
            ModelName = modelName,
            Rank = rank
        };
    }

    private class LeaguesDisabledCupService : ICupService
    {
        public CupOptions GetCupOptions(string cupId) => new() { Leagues = { Enabled = false } };

        public IEnumerable<string> GetEnabledCupIds() => [];

        public bool CupExists(string cupId) => true;

        public IReadOnlyDictionary<string, CupOptions> GetAllCups() =>
            new Dictionary<string, CupOptions>();
    }
}
