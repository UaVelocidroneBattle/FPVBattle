using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Services;

public interface ILeaderboardCalculator
{
    List<CompetitionResults> GetLeaderboard(Competition competition);
    List<LeagueLeaderboard> GetLeagueLeaderboard(Competition competition);
    Task<List<LeagueSeasonLeaderboard>> GetSeasonLeaderboardAsync(string cupId, DateTime from, DateTime to);
}

public class LeaderboardCalculator : ILeaderboardCalculator
{
    private readonly IRepository<Competition> _competitions;
    private readonly ICupService _cupService;
    private readonly PointsCalculator _pointsCalculator;

    public LeaderboardCalculator(
        IRepository<Competition> competitions,
        ICupService cupService,
        PointsCalculator pointsCalculator)
    {
        _competitions = competitions;
        _cupService = cupService;
        _pointsCalculator = pointsCalculator;
    }

    /// <summary>
    /// Returns flat competition results. When leagues are enabled, pilots are ranked within their league group.
    /// </summary>
    public List<CompetitionResults> GetLeaderboard(Competition competition)
    {
        var leaguesEnabled = _cupService.GetCupOptions(competition.CupId).Leagues.Enabled;

        var bestDeltas = competition.TimeDeltas
            .GroupBy(d => d.PilotId)
            .Select(d => SelectBestDelta(d, competition.QuadOfTheDay))
            .OrderBy(d => d.TrackTime)
            .ToList();

        if (!leaguesEnabled)
            return RankFlat(bestDeltas, competition.QuadOfTheDay);

        return bestDeltas
            .GroupBy(d => d.Pilot.GetCurrentLeague(competition.CupId, competition.StartedOn))
            .SelectMany(leagueGroup => RankFlat(leagueGroup.ToList(), competition.QuadOfTheDay))
            .ToList();
    }

    private static TrackTimeDelta SelectBestDelta(IGrouping<int, TrackTimeDelta> pilotDeltas, QuadModel? quadOfTheDay)
    {
        var fastest = pilotDeltas.MinBy(x => x.TrackTime)!;

        if (quadOfTheDay is null)
            return fastest;

        return pilotDeltas
            .Where(x => string.Equals(x.ModelName, quadOfTheDay.Name, StringComparison.OrdinalIgnoreCase))
            .MinBy(x => x.TrackTime) ?? fastest;
    }

    /// <summary>
    /// Ranks pilots by absolute finish time, but spreads points based on rank among pilots who
    /// flew the quad of the day (if any). Pilots who didn't fly it always score 1 point, so a
    /// non-QOD pilot finishing ahead of QOD pilots can no longer take a higher-value points slot.
    /// </summary>
    private List<CompetitionResults> RankFlat(List<TrackTimeDelta> deltas, QuadModel? quadOfTheDay)
    {
        var qodPosition = 0;

        return deltas
            .Select((x, i) =>
            {
                var usedQuadOfTheDay = quadOfTheDay is null ||
                    string.Equals(x.ModelName, quadOfTheDay.Name, StringComparison.OrdinalIgnoreCase);

                var points = usedQuadOfTheDay
                    ? _pointsCalculator.PointsByPosition(++qodPosition)
                    : 1;

                return new CompetitionResults
                {
                    CompetitionId = x.CompetitionId,
                    PilotId = x.PilotId,
                    Pilot = x.Pilot,
                    TrackTime = x.TrackTime,
                    LocalRank = i + 1,
                    GlobalRank = x.Rank,
                    Points = points,
                    ModelName = x.ModelName
                };
            })
            .ToList();
    }

    /// <summary>
    /// Returns the leaderboard grouped by league, with groups ordered according to <see cref="LeagueDescriptor.Order"/> from settings.
    /// </summary>
    public List<LeagueLeaderboard> GetLeagueLeaderboard(Competition competition)
    {
        var cupOptions = _cupService.GetCupOptions(competition.CupId);
        var leagueOrder = cupOptions.Leagues.Definitions.ToDictionary(d => d.Name, d => d.Order);
        var leaderboard = competition.CompetitionResults is { Count: > 0 }
            ? competition.CompetitionResults
            : GetLeaderboard(competition);

        var othersName = cupOptions.Leagues.OthersName;

        var result = leaderboard
            .GroupBy(r => r.Pilot.GetCurrentLeague(competition.CupId, competition.StartedOn))
            .Select(g => new LeagueLeaderboard
            {
                League = g.Key ?? othersName,
                Results = g.OrderBy(r => r.LocalRank).ToList()
            })
            .ToDictionary(l => l.League);

        foreach (var name in cupOptions.Leagues.GetAllLeagueNames().Where(n => !result.ContainsKey(n)))
            result[name] = new LeagueLeaderboard { League = name, Results = [] };

        return result.Values
            .OrderBy(l => leagueOrder.TryGetValue(l.League, out var order) ? order : int.MaxValue)
            .ToList();
    }

    /// <summary>
    /// Returns season leaderboard for the given cup and date range, grouped by league and ordered according to <see cref="LeagueDescriptor.Order"/> from settings.
    /// </summary>
    public async Task<List<LeagueSeasonLeaderboard>> GetSeasonLeaderboardAsync(string cupId, DateTime from, DateTime to)
    {
        var cupOptions = _cupService.GetCupOptions(cupId);
        var leaguesEnabled = cupOptions.Leagues.Enabled;
        var leagueOrder = cupOptions.Leagues.Definitions.ToDictionary(d => d.Name, d => d.Order);

        var pilotResults = await _competitions
            .GetAll(comp => comp.StartedOn >= from && comp.StartedOn <= to)
            .ForCup(cupId)
            .Where(comp => comp.State != CompetitionState.Cancelled)
            .SelectMany(comp => comp.CompetitionResults)
            .GroupBy(result => result.PilotId)
            .Select(group => new
            {
                PlayerName = group.First().Pilot.Name,
                Points = group.Sum(r => r.Points + r.BonusPoints),
                Country = group.First().Pilot.Country,
                League = leaguesEnabled
                    ? group.First().Pilot.Leagues
                        .Where(l => l.CupId == cupId && l.Date.Date <= from.Date)
                        .OrderByDescending(l => l.Date)
                        .Select(l => l.League)
                        .FirstOrDefault()
                    : null
            })
            .ToListAsync();

        var othersName = cupOptions.Leagues.OthersName;

        var result = pilotResults
            .GroupBy(r => r.League)
            .Select(g => new LeagueSeasonLeaderboard
            {
                League = g.Key ?? othersName,
                Results = g
                    .OrderByDescending(r => r.Points)
                    .Select((r, i) => new SeasonResult
                    {
                        PlayerName = r.PlayerName,
                        Points = r.Points,
                        Country = r.Country,
                        Rank = i + 1
                    })
                    .ToList()
            })
            .ToDictionary(l => l.League!);

        foreach (var name in cupOptions.Leagues.GetAllLeagueNames().Where(n => !result.ContainsKey(n)))
            result[name] = new LeagueSeasonLeaderboard { League = name, Results = [] };

        return result.Values
            .OrderBy(l => l.League is not null && leagueOrder.TryGetValue(l.League, out var order) ? order : int.MaxValue)
            .ToList();
    }
}
