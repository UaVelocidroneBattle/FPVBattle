using FpvBattle.Application.API.Velocidrone;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Notifications;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Services;

public class ResultsUpdater
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ResultsUpdater>();

    private readonly IVelocidroneClient _velocidrone;
    private readonly IApplicationDbContext _db;
    private readonly RaceResultsConverter _resultsConverter;
    private readonly RaceResultDeltaAnalyzer _analyzer;
    private readonly IPublisher _publisher;
    private readonly PilotService _pilotService;
    private readonly ICupService _cupService;

    public ResultsUpdater(
        IApplicationDbContext db,
        RaceResultsConverter resultsConverter,
        RaceResultDeltaAnalyzer analyzer,
        IPublisher publisher,
        IVelocidroneClient velocidrone,
        PilotService pilotService,
        ICupService cupService)
    {
        _db = db;
        _resultsConverter = resultsConverter;
        _analyzer = analyzer;
        _publisher = publisher;
        _velocidrone = velocidrone;
        _pilotService = pilotService;
        _cupService = cupService;
    }

    [DisableConcurrentExecution("Competition", 60)]
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = [20, 120])]
    public async Task UpdateResultsAsync()
    {
        var activeCompetitions = await _db.Competitions.Active().ToListAsync();

        if (!activeCompetitions.Any())
        {
            Log.Debug("No active competitions found for result updates");
            return;
        }

        foreach (var competition in activeCompetitions)
        {
            await UpdateResultsAsync(competition);
        }
    }

    private async Task UpdateResultsAsync(Competition competition)
    {
        Log.Debug("Starting updating results for competition {CompetitionId} (cup {CupId}) on track {TrackName}", competition.Id, competition.CupId, competition.Track.Name);

        var cupOptions = _cupService.GetCupOptions(competition.CupId);

        var resultsDto = await _velocidrone.LeaderboardAsync(competition.Track.TrackId);
        var times = await _resultsConverter.ConvertTrackTimesAsync(resultsDto, cupOptions.QuadClasses, competition.QuadOfTheDay);
        Log.Debug("Retrieved {ResultCount} results from Velocidrone API for competition {CompetitionId}", times.Count, competition.Id);

        var results = new TrackResults
        {
            Times = times
        };

        var deltas = _analyzer.CompareResults(competition.CurrentResults, results);
        Log.Debug("Found {DeltaCount} result changes for competition {CompetitionId}", deltas.Count, competition.Id);

        if (!deltas.Any())
        {
            return;
        }

        var pilotNames = times.ToDictionary(x => x.UserId.Value, x => x.PlayerName);
        await _pilotService.UpdatePilotsAsync(deltas, pilotNames, competition.CupId);

        competition.CurrentResults = results;
        competition.TimeDeltas.AddRange(deltas);
        competition.ResultsPosted = false;
        await _db.SaveChangesAsync();

        Log.Information("Updated results for competition {CompetitionId} (cup {CupId}): {DeltaCount} new results added", competition.Id, competition.CupId, deltas.Count);
        await _publisher.Publish(new CurrentResultUpdated(competition, deltas, cupOptions));
    }
}
