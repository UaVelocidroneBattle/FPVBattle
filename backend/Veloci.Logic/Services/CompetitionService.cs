using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.Bot;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Notifications;

namespace Veloci.Logic.Services;

public class CompetitionService
{
    private static readonly ILogger _log = Log.ForContext<CompetitionService>();

    private readonly Velocidrone _velocidrone;
    private readonly IRepository<Competition> _competitions;
    private readonly RaceResultsConverter _resultsConverter;
    private readonly RaceResultDeltaAnalyzer _analyzer;
    private readonly IMediator _mediator;
    private readonly PilotService _pilotService;
    private readonly ICupService _cupService;
    private readonly ILeaderboardCalculator  _leaderboardCalculator;

    public CompetitionService(
        IRepository<Competition> competitions,
        RaceResultsConverter resultsConverter,
        RaceResultDeltaAnalyzer analyzer,
        IMediator mediator,
        Velocidrone velocidrone,
        PilotService pilotService,
        ICupService cupService,
        ILeaderboardCalculator leaderboardCalculator)
    {
        _competitions = competitions;
        _resultsConverter = resultsConverter;
        _analyzer = analyzer;
        _mediator = mediator;
        _velocidrone = velocidrone;
        _pilotService = pilotService;
        _cupService = cupService;
        _leaderboardCalculator = leaderboardCalculator;
    }

    [DisableConcurrentExecution("Competition", 60)]
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = [20, 120])]
    public async Task UpdateResultsAsync()
    {
        var activeCompetitions = await _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .ToListAsync();

        if (!activeCompetitions.Any())
        {
            _log.Debug("No active competitions found for result updates");
            return;
        }

        foreach (var competition in activeCompetitions)
        {
            await UpdateResultsAsync(competition);
        }
    }

    private async Task UpdateResultsAsync(Competition competition)
    {
        _log.Debug("Starting updating results for competition {CompetitionId} (cup {CupId}) on track {TrackName}", competition.Id, competition.CupId, competition.Track.Name);

        var cupOptions = _cupService.GetCupOptions(competition.CupId);

        var resultsDto = await _velocidrone.LeaderboardAsync(competition.Track.TrackId);
        var times = await _resultsConverter.ConvertTrackTimesAsync(resultsDto, cupOptions.QuadClasses, competition.QuadOfTheDay);
        _log.Debug("Retrieved {ResultCount} results from Velocidrone API for competition {CompetitionId}", times.Count, competition.Id);

        var results = new TrackResults
        {
            Times = times
        };

        var deltas = _analyzer.CompareResults(competition.CurrentResults, results);
        _log.Debug("Found {DeltaCount} result changes for competition {CompetitionId}", deltas.Count, competition.Id);

        if (!deltas.Any())
        {
            return;
        }

        var pilotNames = times.ToDictionary(x => x.UserId.Value, x => x.PlayerName);
        await _pilotService.UpdatePilotsAsync(deltas, pilotNames, competition.CupId);

        competition.CurrentResults = results;
        competition.TimeDeltas.AddRange(deltas);
        competition.ResultsPosted = false;
        await _competitions.SaveChangesAsync();

        _log.Information("Updated results for competition {CompetitionId} (cup {CupId}): {DeltaCount} new results added", competition.Id, competition.CupId, deltas.Count);
        await _mediator.Publish(new CurrentResultUpdated(competition, deltas, cupOptions));
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [20, 120, 600])]
    [DisableConcurrentExecution("Competition", 1)]
    public async Task PublishCurrentLeaderboardAsync()
    {
        var activeCompetitions = await GetCurrentCompetitions().ToListAsync();

        if (!activeCompetitions.Any())
        {
            _log.Debug("No active competitions found for leaderboard publishing");
            return;
        }

        foreach (var activeCompetition in activeCompetitions)
        {
            await PublishCurrentLeaderboardAsync(activeCompetition);
        }
    }

    private async Task PublishCurrentLeaderboardAsync(Competition competition)
    {
        if (competition.ResultsPosted)
        {
            _log.Debug("Leaderboard already published for competition {CompetitionId}, skipping", competition.Id);
            return;
        }

        if (competition.TimeDeltas.Count == 0)
        {
            _log.Information("No results yet for competition {CompetitionId}", competition.Id);
            await SendCheerUpMessageAsync(competition.CupId, ChatMessageType.NobodyFlying);
            return;
        }

        var leaderboard = _leaderboardCalculator.GetLeagueLeaderboard(competition);

        if (leaderboard.Sum(x => x.Results.Count) < 2)
        {
            await SendCheerUpMessageAsync(competition.CupId, ChatMessageType.OnlyOneFlew);
            return;
        }

        _log.Information("🏆 Publishing leaderboard for competition {CompetitionId} with {ResultCount} results", competition.Id, leaderboard.Count);

        await _mediator.Publish(new IntermediateCompetitionResult(leaderboard, competition));

        competition.ResultsPosted = true;
        await _competitions.SaveChangesAsync();
        _log.Debug("Marked leaderboard as published for competition {CompetitionId}", competition.Id);
    }

    private async Task SendCheerUpMessageAsync(string cupId, ChatMessageType type)
    {
        if (DoNotDisturb(DateTime.Now))
        {
            _log.Debug("Skipping cheer-up message due to do-not-disturb hours (current time: {CurrentTime})", DateTime.Now.ToString("HH:mm"));
            return;
        }

        await _mediator.Publish(new CheerUp(cupId, type));
    }

    private static bool DoNotDisturb(DateTime dateTime)
    {
        return dateTime.Hour is < 7 or > 22;
    }

    public IQueryable<Competition> GetCurrentCompetitions()
    {
        return _competitions
            .GetAll(c => c.State == CompetitionState.Started)
            .OrderByDescending(x => x.StartedOn);
    }

    public IQueryable<Competition> GetCompetitionsForDate(DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = date.ToDateTime(TimeOnly.MaxValue);

        return _competitions
            .GetAll(c => c.StartedOn >= dayStart && c.StartedOn <= dayEnd)
            .OrderByDescending(x => x.StartedOn);
    }
}
