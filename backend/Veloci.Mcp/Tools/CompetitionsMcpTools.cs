using System.ComponentModel;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
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
    private readonly IRepository<Competition> _competitionRepository;
    private readonly IMediator _mediator;

    public CompetitionsMcpTools(
        CompetitionService competitionService,
        IRepository<Competition> competitionRepository,
        CompetitionConductor competitionConductor,
        IMediator mediator)
    {
        _competitionService = competitionService;
        _competitionRepository = competitionRepository;
        _competitionConductor = competitionConductor;
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get a competition by starting date. One competition lasts one day. Current competition started today or yesterday depending on current time. Returns competition details including track name, start time, state (Open/Closed), and current results.")]
    public async Task<CompetitionDto?> GetCompetition([Description("Date when the competition started")]DateTime startDate)
    {
        var competition = await FetchCompetitionEntityAsync(startDate);

        if (competition is null)
            return null;

        return CompetitionDto.FromEntity(competition);
    }

    [McpServerTool]
    [Description("Get leaderboard for a competition by date. Returns ranked list of pilots with their lap times (in milliseconds), local/global ranks, earned points, and drone model used.")]
    public async Task<List<CompetitionResults>> GetLeaderboardForCompetition([Description("Date when the competition started")]DateTime startDate)
    {
        Log.Information("Fetching leaderboard of the competition for date {Date}", startDate);

        var competition = await FetchCompetitionEntityAsync(startDate);

        if (competition is null)
        {
            Log.Warning("No competition found for date {Date}", startDate);
            return [];
        }

        var leaderboard = _competitionService.GetLocalLeaderboard(competition);

        Log.Information("Returning leaderboard with {PilotCount} pilots for competition {CompetitionId}",
            leaderboard.Count, competition.Id);

        return leaderboard;
    }

    private async Task<Competition?> FetchCompetitionEntityAsync(DateTime startDate)
    {
        Log.Information("Fetching competition for date {Date}", startDate);

        var startDay = startDate.Date;
        var nextDay = startDay.AddDays(1);

        var competition = await _competitionRepository
            .GetAll()
            .NotCancelled()
            .InRange(startDay, nextDay)
            .FirstOrDefaultAsync();

        if (competition is null)
        {
            Log.Warning("No competition found for date {Date}", startDate);
            return null;
        }

        Log.Information("Found competition {CompetitionId} on track {TrackName}, state: {State}",
            competition.Id, competition.Track.Name, competition.State);

        return competition;
    }

    [McpServerTool]
    [Description("Get season leaderboard for a date range. Returns accumulated pilot rankings with total points and medal counts (gold/silver/bronze placements). Typically a season is one calendar month.")]
    public async Task<List<SeasonResult>> GetSeasonLeaderboard(
        [Description("Season start date (inclusive), e.g., first day of month")] DateTime from,
        [Description("Season end date (inclusive), e.g., last day of month or today for current season")] DateTime to)
    {
        Log.Information("Fetching season leaderboard from {From} to {To}", from, to);

        var results = await _competitionService.GetSeasonResultsAsync(from, to);

        Log.Information("Returning season leaderboard with {PilotCount} pilots", results.Count);

        return results;
    }

    [McpServerTool]
    [Description("Cancelling a current competition and starting a new one to pick a new track")]
    public async Task ChangeTrack()
    {
        await _mediator.Publish(new TrackRestart());
        BackgroundJob.Enqueue(() => _competitionConductor.StartNewAsync());
    }
}
