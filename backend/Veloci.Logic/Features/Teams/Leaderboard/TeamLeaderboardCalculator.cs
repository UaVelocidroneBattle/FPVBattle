using Microsoft.Extensions.Options;
using Veloci.Data.Domain;
using Veloci.Logic.Services;

namespace Veloci.Logic.Features.Teams.Leaderboard;

/// <summary>
/// A team's time on the daily track is the average time of its pilots who flew it.
/// </summary>
public class TeamLeaderboardCalculator
{
    private readonly PointsCalculator _pointsCalculator;
    private readonly TeamSettings _settings;

    public TeamLeaderboardCalculator(PointsCalculator pointsCalculator, IOptions<TeamSettings> settings)
    {
        _pointsCalculator = pointsCalculator;
        _settings = settings.Value;
    }

    public IReadOnlyDictionary<int, List<CompetitionResults>> GroupByTeam(
        IEnumerable<CompetitionResults> results,
        QuadModel? quadOfTheDay,
        IReadOnlyDictionary<int, int> teamIdsByPilotId)
    {
        return results
            .ForQuadOfTheDay(quadOfTheDay)
            .Where(r => teamIdsByPilotId.ContainsKey(r.PilotId))
            .GroupBy(r => teamIdsByPilotId[r.PilotId])
            .ToDictionary(team => team.Key, team => team.OrderBy(r => r.TrackTime).ToList());
    }

    public List<TeamCompetitionResult> Rank(string competitionId, IReadOnlyDictionary<int, List<CompetitionResults>> pilotsByTeam)
    {
        return pilotsByTeam
            .Where(team => team.Value.Count >= _settings.MinTeamFinishers)
            .Select(team => new
            {
                TeamId = team.Key,
                AverageTime = (int)Math.Round(team.Value.Average(r => r.TrackTime)),
                Pilots = team.Value.Count
            })
            .OrderBy(team => team.AverageTime)
            .ThenByDescending(team => team.Pilots)
            .Select((team, i) => new TeamCompetitionResult
            {
                CompetitionId = competitionId,
                TeamId = team.TeamId,
                AverageTime = team.AverageTime,
                Rank = i + 1,
                Points = _pointsCalculator.PointsByPosition(i + 1)
            })
            .ToList();
    }
}
