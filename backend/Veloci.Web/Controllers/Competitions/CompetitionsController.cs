using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services;

namespace Veloci.Web.Controllers.Competitions;

[ApiController]
[Route("/api/competitions/[action]")]
public class CompetitionsController : ControllerBase
{
    private readonly CompetitionService _competitionService;
    private readonly ICupService _cupService;
    private readonly ILeaderboardCalculator _leaderboardCalculator;

    public CompetitionsController(CompetitionService competitionService, ICupService cupService, ILeaderboardCalculator leaderboardCalculator)
    {
        _competitionService = competitionService;
        _cupService = cupService;
        _leaderboardCalculator = leaderboardCalculator;
    }

    [HttpGet("/api/competitions/current")]
    public async Task<CompetitionModel[]> GetCurrent()
    {
        var competitions  = await _competitionService.GetCurrentCompetitions().ProjectToModel().ToArrayAsync() ;
        return competitions;
    }

    [HttpGet("/api/dashboard")]
    public async Task<DashboardModel?> Dashboard([FromQuery] string? cupId = null, [FromQuery] DateOnly? date = null)
    {
        // Default to first enabled cup if not specified
        cupId ??= _cupService.GetEnabledCupIds().FirstOrDefault() ?? CupIds.OpenClass;

        var competitionsQuery = date.HasValue
            ? _competitionService.GetCompetitionsForDate(date.Value)
            : _competitionService.GetCurrentCompetitions();

        var competition = await competitionsQuery
            .ForCup(cupId)
            .FirstOrDefaultAsync();

        var cupOptions = _cupService.GetCupOptions(cupId);

        var dashboardModel = new DashboardModel
        {
            Competition = ToCompetitionModel(competition, cupOptions),
            Leaderboard = GetLeaderboard(competition),
            SeasonLeaderboard = await GetSeasonLeaderboardAsync(cupId, date)
        };

        return dashboardModel;
    }

    private List<LeagueLeaderboardModel> GetLeaderboard(Competition? competition)
    {
        if (competition is null)
            return [];

        return _leaderboardCalculator.GetLeagueLeaderboard(competition)
            .Select(ToLeaderboardModel)
            .ToList();
    }

    private static LeagueLeaderboardModel ToLeaderboardModel(LeagueLeaderboard league) =>
        new()
        {
            League = league.League,
            Results = league.Results.Select(r => new LeaderboardResultModel
            {
                PlayerName = r.Pilot.Name,
                TrackTime = r.TrackTime,
                LocalRank = r.LocalRank,
                GlobalRank = r.GlobalRank,
                ModelName = r.ModelName,
                Country = r.Pilot.Country
            }).ToList()
        };

    private async Task<List<LeagueSeasonLeaderboard>> GetSeasonLeaderboardAsync(string cupId, DateOnly? date = null)
    {
        var to = date.HasValue
            ? date.Value.ToDateTime(TimeOnly.MaxValue)
            : DateTime.Now;
        var from = new DateTime(to.Year, to.Month, 1);

        return await _leaderboardCalculator.GetSeasonLeaderboardAsync(cupId, from, to);
    }

    private CompetitionModel? ToCompetitionModel(Competition? competition, CupOptions cupOptions)
    {
        if (competition is null)
            return null;

        return new CompetitionModel
        {
            Id = competition.Id,
            MapId = competition.Track.Map.MapId,
            MapName =  competition.Track.Map.Name,
            TrackId = competition.Track.TrackId,
            TrackName =  competition.Track.Name,
            StartedOn = competition.StartedOn,
            State = competition.State,
            QuadOfTheDay = competition.QuadOfTheDay?.Name,
            Leagues = cupOptions.Leagues
        };
    }
}
