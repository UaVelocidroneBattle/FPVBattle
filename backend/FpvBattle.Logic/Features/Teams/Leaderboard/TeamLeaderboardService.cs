using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Services;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Logic.Features.Teams.Leaderboard;

public record TeamDailyStanding(Team Team, int AverageTime, int Rank, int Points, IReadOnlyList<CompetitionResults> Pilots);

public record TeamSeasonStanding(Team Team, int Points, int Rank);

/// <summary>
/// Team leaderboards for the website. Deleted teams stay in past results, so these queries ignore the team query filter.
/// </summary>
public class TeamLeaderboardService
{
    private readonly IApplicationDbContext _db;
    private readonly ILeaderboardCalculator _leaderboardCalculator;
    private readonly TeamLeaderboardCalculator _calculator;
    private readonly TeamRosterLookup _roster;

    public TeamLeaderboardService(
        IApplicationDbContext db,
        ILeaderboardCalculator leaderboardCalculator,
        TeamLeaderboardCalculator calculator,
        TeamRosterLookup roster)
    {
        _db = db;
        _leaderboardCalculator = leaderboardCalculator;
        _calculator = calculator;
        _roster = roster;
    }

    /// <summary>
    /// A running competition is ranked live. A finished one shows its stored results, with each team's pilots
    /// taken from today's membership, so for an old day they may not add up to the stored average.
    /// </summary>
    public async Task<List<TeamDailyStanding>> GetDailyAsync(Competition competition, CancellationToken ct = default)
    {
        var isRunning = competition.State == CompetitionState.Started;
        var pilotResults = isRunning ? _leaderboardCalculator.GetLeaderboard(competition) : competition.CompetitionResults;

        var roster = await _roster.GetTeamIdsByPilotIdAsync(ct);
        var pilotsByTeam = _calculator.GroupByTeam(pilotResults, competition.QuadOfTheDay, roster);

        var teamResults = isRunning
            ? _calculator.Rank(competition.Id, pilotsByTeam)
            : await _db.TeamCompetitionResults.IgnoreQueryFilters().Where(r => r.CompetitionId == competition.Id).ToListAsync(ct);

        var teams = await FindTeamsAsync(teamResults.Select(r => r.TeamId), ct);

        return teamResults
            .OrderBy(r => r.Rank)
            .Select(r => new TeamDailyStanding(teams[r.TeamId], r.AverageTime, r.Rank, r.Points, pilotsByTeam.GetValueOrDefault(r.TeamId) ?? []))
            .ToList();
    }

    /// <summary>
    /// The team leaderboard to store with a closing competition, from its final results and team membership at this moment.
    /// </summary>
    public async Task<List<TeamCompetitionResult>> RankFinishedAsync(Competition competition, CancellationToken ct = default)
    {
        var roster = await _roster.GetTeamIdsByPilotIdAsync(ct);
        var pilotsByTeam = _calculator.GroupByTeam(competition.CompetitionResults, competition.QuadOfTheDay, roster);

        return _calculator.Rank(competition.Id, pilotsByTeam);
    }

    public async Task<List<TeamSeasonStanding>> GetSeasonAsync(string cupId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var teamPoints = await _db.Competitions.Where(c => c.StartedOn >= from && c.StartedOn <= to)
            .IgnoreQueryFilters()
            .ForCup(cupId)
            .NotCancelled()
            .SelectMany(c => c.TeamCompetitionResults)
            .GroupBy(r => r.TeamId)
            .Select(team => new { TeamId = team.Key, Points = team.Sum(r => r.Points) })
            .ToListAsync(ct);

        var teams = await FindTeamsAsync(teamPoints.Select(t => t.TeamId), ct);

        return teamPoints
            .OrderByDescending(t => t.Points)
            .Select((t, i) => new TeamSeasonStanding(teams[t.TeamId], t.Points, i + 1))
            .ToList();
    }

    private async Task<Dictionary<int, Team>> FindTeamsAsync(IEnumerable<int> teamIds, CancellationToken ct)
    {
        var ids = teamIds.ToList();

        return await _db.Teams
            .IgnoreQueryFilters()
            .Where(t => ids.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, ct);
    }
}
