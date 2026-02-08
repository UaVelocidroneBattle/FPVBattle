using System.ComponentModel;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services;
using Veloci.Mcp.Dto;

namespace Veloci.Mcp.Tools;

[McpServerToolType]
public class CompetitionsMcpTools
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CompetitionsMcpTools>();
    private readonly CompetitionService _competitionService;
    private readonly CompetitionConductor _competitionConductor;
    private readonly CupService _cupService;
    private readonly IRepository<Competition> _competitionRepository;
    private readonly IMediator _mediator;

    public CompetitionsMcpTools(
        CompetitionService competitionService,
        IRepository<Competition> competitionRepository,
        CompetitionConductor competitionConductor,
        IMediator mediator,
        CupService cupService)
    {
        _competitionService = competitionService;
        _competitionRepository = competitionRepository;
        _competitionConductor = competitionConductor;
        _mediator = mediator;
        _cupService = cupService;
    }

    [McpServerTool]
    [Description("Get all enabled cup IDs configured in the system")]
    public IEnumerable<string> GetCupIds()
    {
        Log.Information("Fetching enabled cup IDs");
        var cupIds = _cupService.GetEnabledCupIds().ToList();
        Log.Information("Found {Count} enabled cups", cupIds.Count);
        return cupIds;
    }

    [McpServerTool]
    [Description("Get a competition by starting date. One competition lasts one day. Current competition started today or yesterday depending on current time. Returns competition details including track name, start time, state (Open/Closed), and current results.")]
    public async Task<CompetitionDto?> GetCompetition(
        [Description("Cup unique id, e.g. 'open-class' or 'whoop-class'")] string cupId,
        [Description("Date when the competition started")] DateTime startDate)
    {
        Log.Information("Getting competition for cup {CupId} on date {Date}", cupId, startDate);

        var competition = await FetchCompetitionEntityAsync(cupId, startDate);

        if (competition is null)
        {
            Log.Warning("No competition found for cup {CupId} on date {Date}", cupId, startDate);
            return null;
        }

        Log.Information("Returning competition {CompetitionId} for cup {CupId}", competition.Id, cupId);
        return CompetitionDto.FromEntity(competition);
    }

    [McpServerTool]
    [Description("Get leaderboard for a competition by date. Returns ranked list of pilots with their lap times (in milliseconds), local/global ranks, earned points, and drone model used.")]
    public async Task<List<CompetitionResults>> GetLeaderboardForCompetition(
        [Description("Cup unique id, e.g. 'open-class' or 'whoop-class'")] string cupId,
        [Description("Date when the competition started")] DateTime startDate)
    {
        Log.Information("Fetching leaderboard for cup {CupId} on date {Date}", cupId, startDate);

        var competition = await FetchCompetitionEntityAsync(cupId, startDate);

        if (competition is null)
        {
            Log.Warning("No competition found for cup {CupId} on date {Date}", cupId, startDate);
            return [];
        }

        var leaderboard = _competitionService.GetLocalLeaderboard(competition);

        Log.Information("Returning leaderboard with {PilotCount} pilots for cup {CupId}, competition {CompetitionId}",
            leaderboard.Count, cupId, competition.Id);

        return leaderboard;
    }

    private async Task<Competition?> FetchCompetitionEntityAsync(string cupId, DateTime startDate)
    {
        Log.Information("Fetching competition for cup {CupId} on date {Date}", cupId, startDate);

        var startDay = startDate.Date;
        var nextDay = startDay.AddDays(1);

        var competition = await _competitionRepository
            .GetAll()
            .NotCancelled()
            .ForCup(cupId)
            .InRange(startDay, nextDay)
            .FirstOrDefaultAsync();

        if (competition is null)
        {
            Log.Warning("No competition found for cup {CupId} on date {Date}", cupId, startDate);
            return null;
        }

        Log.Information("Found competition {CompetitionId} for cup {CupId} on track {TrackName}, state: {State}",
            competition.Id, cupId, competition.Track.Name, competition.State);

        return competition;
    }

    [McpServerTool]
    [Description("Get season leaderboard for a date range. Returns accumulated pilot rankings with total points and medal counts (gold/silver/bronze placements). Typically a season is one calendar month.")]
    public async Task<List<SeasonResult>> GetSeasonLeaderboard(
        [Description("Cup unique id, e.g. 'open-class' or 'whoop-class'")] string cupId,
        [Description("Season start date (inclusive), e.g., first day of month")] DateTime from,
        [Description("Season end date (inclusive), e.g., last day of month or today for current season")] DateTime to)
    {
        Log.Information("Fetching season leaderboard for cup {CupId} from {From} to {To}", cupId, from, to);

        var results = await _competitionService.GetSeasonResultsAsync(cupId, from, to);

        Log.Information("Returning season leaderboard with {PilotCount} pilots for cup {CupId}", results.Count, cupId);

        return results;
    }

    [McpServerTool]
    [Description("Cancel the current competition and start a new one with a different track")]
    public async Task ChangeTrack([Description("Cup unique id, e.g. 'open-class' or 'whoop-class'")] string cupId)
    {
        Log.Information("Changing track for cup {CupId}", cupId);

        await _mediator.Publish(new TrackRestart(cupId));
        BackgroundJob.Enqueue(() => _competitionConductor.StartNewAsync(cupId));

        Log.Information("Track change initiated for cup {CupId}, new competition scheduled", cupId);
    }
}
