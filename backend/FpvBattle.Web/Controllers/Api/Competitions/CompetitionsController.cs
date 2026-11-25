using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FpvBattle.Application.Features.Auth;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Features.Teams;
using FpvBattle.Application.Features.Teams.Leaderboard;
using FpvBattle.Application.Services;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;
using FpvBattle.Application.Persistence.Queries;

namespace FpvBattle.Web.Controllers.Api.Competitions;

[ApiController]
[Route("/api/competitions/[action]")]
public class CompetitionsController : ControllerBase
{
    private readonly CompetitionService _competitionService;
    private readonly ICupService _cupService;
    private readonly ILeaderboardCalculator _leaderboardCalculator;
    private readonly TeamTagLookup _teamTags;
    private readonly TeamLeaderboardService _teamLeaderboard;
    private readonly TrackRestartService _trackRestartService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CompetitionsController(
        CompetitionService competitionService,
        ICupService cupService,
        ILeaderboardCalculator leaderboardCalculator,
        TeamTagLookup teamTags,
        TeamLeaderboardService teamLeaderboard,
        TrackRestartService trackRestartService,
        UserManager<ApplicationUser> userManager)
    {
        _competitionService = competitionService;
        _cupService = cupService;
        _leaderboardCalculator = leaderboardCalculator;
        _teamTags = teamTags;
        _teamLeaderboard = teamLeaderboard;
        _trackRestartService = trackRestartService;
        _userManager = userManager;
    }

    [HttpGet("/api/competitions/current")]
    public async Task<CompetitionModel[]> GetCurrent()
    {
        var competitions  = await _competitionService.GetCurrentCompetitions().ProjectToModel().ToArrayAsync() ;
        return competitions;
    }

    /// <summary>
    /// Cancels the cup's running competition and starts a new one the same way the daily start does.
    /// </summary>
    [HttpPost("/api/competitions/restart-track")]
    [Authorize(Policy = AuthPolicies.CanRestartTrack)]
    public async Task<IActionResult> RestartTrack([FromQuery] string cupId)
    {
        if (!_cupService.CupExists(cupId))
            return NotFound();

        // Conductors replace a running track; only admins (MCP, Telegram) may start a missing competition
        var isRunning = await _competitionService.GetCurrentCompetitions().ForCup(cupId).AnyAsync();
        if (!isRunning)
            return Conflict("There is no running competition to restart");

        var user = await _userManager.GetUserAsync(User);
        var result = await _trackRestartService.RestartAsync(cupId, restartedBy: user?.Pilot?.Name);

        return result switch
        {
            TrackRestartResult.Restarted => NoContent(),
            TrackRestartResult.RestartedMomentsAgo => Conflict("The track was restarted moments ago"),
            _ => NotFound()
        };
    }

    [HttpGet("/api/competitions/overview")]
    public async Task<CompetitionOverviewModel?> Overview([FromQuery] string? cupId = null, [FromQuery] DateOnly? date = null)
    {
        cupId ??= DefaultCupId();
        var competition = await FindCompetitionAsync(cupId, date);
        var cupOptions = _cupService.GetCupOptions(cupId);

        var teamTags = await _teamTags.GetTagsByPilotNameAsync();
        var seasonLeaderboard = await GetSeasonLeaderboardAsync(cupId, date);

        foreach (var result in seasonLeaderboard.SelectMany(league => league.Results))
            result.TeamTag = teamTags.GetValueOrDefault(result.PlayerName);

        var overview = new CompetitionOverviewModel
        {
            Competition = ToCompetitionModel(competition, cupOptions),
            Leaderboard = GetLeaderboard(competition, teamTags),
            SeasonLeaderboard = seasonLeaderboard
        };

        return overview;
    }

    [HttpGet("/api/competitions/team-overview")]
    public async Task<TeamOverviewModel> TeamOverview([FromQuery] string? cupId = null, [FromQuery] DateOnly? date = null)
    {
        cupId ??= DefaultCupId();
        var competition = await FindCompetitionAsync(cupId, date);
        var (from, to) = SeasonPeriod(date);

        var leaderboard = competition is null ? [] : await _teamLeaderboard.GetDailyAsync(competition);
        var seasonLeaderboard = await _teamLeaderboard.GetSeasonAsync(cupId, from, to);

        return new TeamOverviewModel
        {
            Leaderboard = leaderboard.Select(TeamLeaderboardMapper.ToModel).ToList(),
            SeasonLeaderboard = seasonLeaderboard.Select(TeamLeaderboardMapper.ToModel).ToList()
        };
    }

    private string DefaultCupId() => _cupService.GetEnabledCupIds().FirstOrDefault() ?? CupIds.OpenClass;

    private async Task<Competition?> FindCompetitionAsync(string cupId, DateOnly? date)
    {
        var competitions = date.HasValue
            ? _competitionService.GetCompetitionsForDate(date.Value)
            : _competitionService.GetCurrentCompetitions();

        return await competitions.ForCup(cupId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// The season is the calendar month of <paramref name="date"/>, up to that day. Without a date, the current month up to now.
    /// </summary>
    private static (DateTime From, DateTime To) SeasonPeriod(DateOnly? date)
    {
        var to = date.HasValue
            ? date.Value.ToDateTime(TimeOnly.MaxValue)
            : DateTime.Now;

        return (new DateTime(to.Year, to.Month, 1), to);
    }

    private List<LeagueLeaderboardModel> GetLeaderboard(Competition? competition, IReadOnlyDictionary<string, string> teamTags)
    {
        if (competition is null)
            return [];

        return _leaderboardCalculator.GetLeagueLeaderboard(competition)
            .Select(league => ToLeaderboardModel(league, teamTags))
            .ToList();
    }

    private static LeagueLeaderboardModel ToLeaderboardModel(LeagueLeaderboard league, IReadOnlyDictionary<string, string> teamTags) =>
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
                Country = r.Pilot.Country,
                TeamTag = teamTags.GetValueOrDefault(r.Pilot.Name),
                Points = r.Points,
                GapToLeaderPercent = r.GapToLeaderPercent
            }).ToList()
        };

    private async Task<List<LeagueSeasonLeaderboard>> GetSeasonLeaderboardAsync(string cupId, DateOnly? date = null)
    {
        var (from, to) = SeasonPeriod(date);

        return await _leaderboardCalculator.GetSeasonLeaderboardAsync(cupId, from, to);
    }

    private CompetitionModel? ToCompetitionModel(Competition? competition, CupOptions cupOptions)
    {
        if (competition is null)
            return null;

        return new CompetitionModel
        {
            Id = competition.Id,
            SceneId = competition.Track.Scene.SceneId,
            SceneName = competition.Track.Scene.Name,
            TrackId = competition.Track.TrackId,
            TrackName = competition.Track.Name,
            StartedOn = competition.StartedOn,
            State = competition.State,
            QuadOfTheDay = competition.QuadOfTheDay?.Name,
            Leagues = cupOptions.Leagues
        };
    }
}
