using FluentAssertions;
using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Teams;
using Veloci.Logic.Features.Teams.Leaderboard;
using Veloci.Logic.Services;

namespace Veloci.Tests;

public class TeamLeaderboardCalculatorTests
{
    private const int Alpha = 1;
    private const int Bravo = 2;
    private const int Charlie = 3;

    private readonly TeamLeaderboardCalculator _calculator =
        new(new PointsCalculator(), Options.Create(new TeamSettings { MinTeamFinishers = 2 }));

    [Fact]
    public void Rank_AveragesEveryPilotWhoFlewAndScoresByPosition()
    {
        var pilotsByTeam = new Dictionary<int, List<CompetitionResults>>
        {
            [Alpha] = [Result(1, 60_000), Result(2, 61_001), Result(3, 61_000)],
            [Bravo] = [Result(4, 59_000), Result(5, 63_000)]
        };

        var results = _calculator.Rank("competition", pilotsByTeam);

        results.Should().BeEquivalentTo(new[]
        {
            new { TeamId = Alpha, AverageTime = 60_667, Rank = 1, Points = 100, CompetitionId = "competition" },
            new { TeamId = Bravo, AverageTime = 61_000, Rank = 2, Points = 85, CompetitionId = "competition" }
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Rank_SkipsTeamsWithTooFewPilots()
    {
        var pilotsByTeam = new Dictionary<int, List<CompetitionResults>>
        {
            [Alpha] = [Result(1, 50_000)],
            [Bravo] = [Result(2, 60_000), Result(3, 60_000)]
        };

        _calculator.Rank("competition", pilotsByTeam)
            .Should().ContainSingle(r => r.TeamId == Bravo && r.Rank == 1 && r.Points == 100);
    }

    [Fact]
    public void Rank_GivesAnEqualAverageToTheTeamWithMorePilots()
    {
        var pilotsByTeam = new Dictionary<int, List<CompetitionResults>>
        {
            [Alpha] = [Result(1, 59_000), Result(2, 61_000)],
            [Bravo] = [Result(3, 60_000), Result(4, 60_000), Result(5, 60_000)]
        };

        _calculator.Rank("competition", pilotsByTeam).Select(r => r.TeamId).Should().Equal(Bravo, Alpha);
    }

    [Fact]
    public void GroupByTeam_CountsOnlyQuadOfTheDayTimesAndPilotsInATeam()
    {
        var quadOfTheDay = new QuadModel { Id = 1, Name = "Tiny Trainer" };
        var roster = new Dictionary<int, int> { [1] = Alpha, [2] = Alpha, [3] = Charlie };
        var results = new[]
        {
            Result(1, 60_000, "tiny trainer"),
            Result(2, 55_000, "Some Faster Quad"),
            Result(3, 62_000, "Tiny Trainer"),
            Result(4, 50_000, "Tiny Trainer")
        };

        var pilotsByTeam = _calculator.GroupByTeam(results, quadOfTheDay, roster);

        pilotsByTeam.Should().HaveCount(2);
        pilotsByTeam[Alpha].Select(r => r.PilotId).Should().Equal(1);
        pilotsByTeam[Charlie].Select(r => r.PilotId).Should().Equal(3);
    }

    private static CompetitionResults Result(int pilotId, int trackTime, string? modelName = null) => new()
    {
        PilotId = pilotId,
        TrackTime = trackTime,
        ModelName = modelName
    };
}
