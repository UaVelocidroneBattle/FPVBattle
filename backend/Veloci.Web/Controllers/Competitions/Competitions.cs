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

    public CompetitionsController(CompetitionService competitionService, ICupService cupService)
    {
        _competitionService = competitionService;
        _cupService = cupService;
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

        var dashboardModel = new DashboardModel
        {
            Competition = competition?.MapToModel(),
            Results = GetCurrentResults(competition),
            Leaderboard = GetLeaderboard(cupId, date)
        };

        return dashboardModel;
    }

    private List<TrackTimeModel> GetCurrentResults(Competition? competition)
    {
        if (competition is null)
            return [];

        var currentResults = _competitionService.GetLocalLeaderboard(competition);
        return currentResults.MapToModel();
    }

    private List<SeasonResultModel> GetLeaderboard(string cupId, DateOnly? date = null)
    {
        var to = date.HasValue
            ? date.Value.ToDateTime(TimeOnly.MaxValue)
            : DateTime.Now;
        var firstDayOfMonth = new DateTime(to.Year, to.Month, 1);

        var leaderboard = _competitionService
            .GetSeasonResultsQuery(cupId, firstDayOfMonth, to)
            .OrderByDescending(result => result.Points)
            .MapToModel();

        return leaderboard;
    }
}
