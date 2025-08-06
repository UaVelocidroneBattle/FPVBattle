using System.Linq.Expressions;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.Bot;
using Veloci.Logic.Notifications;
using Veloci.Logic.Features.Achievements.Notifications;

namespace Veloci.Logic.Services;

public class CompetitionService
{
    private static readonly ILogger _log = Log.ForContext<CompetitionService>();

    private readonly Velocidrone _velocidrone;
    private readonly IRepository<Competition> _competitions;
    private readonly IRepository<Pilot> _pilots;
    private readonly RaceResultsConverter _resultsConverter;
    private readonly RaceResultDeltaAnalyzer _analyzer;
    private readonly IMediator _mediator;

    public CompetitionService(
        IRepository<Competition> competitions,
        RaceResultsConverter resultsConverter,
        RaceResultDeltaAnalyzer analyzer,
        IMediator mediator,
        IRepository<Pilot> pilots,
        Velocidrone velocidrone)
    {
        _competitions = competitions;
        _resultsConverter = resultsConverter;
        _analyzer = analyzer;
        _mediator = mediator;
        _pilots = pilots;
        _velocidrone = velocidrone;
    }

    [DisableConcurrentExecution("Competition", 60)]
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
        _log.Debug("Starting updating results for competition {CompetitionId} on track {TrackName}", competition.Id, competition.Track.Name);

        var resultsDto = await _velocidrone.LeaderboardAsync(competition.Track.TrackId);
        var times = _resultsConverter.ConvertTrackTimes(resultsDto);
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

        competition.CurrentResults = results;
        competition.TimeDeltas.AddRange(deltas);
        competition.ResultsPosted = false;
        await _competitions.SaveChangesAsync();

        _log.Information("Updated results for competition {CompetitionId}: {DeltaCount} new results added", competition.Id, deltas.Count);
        await _mediator.Publish(new CurrentResultUpdateMessage(competition, deltas));
    }

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
            await SendCheerUpMessageAsync(ChatMessageType.NobodyFlying);
            return;
        }

        var leaderboard = GetLocalLeaderboard(competition);

        if (leaderboard.Count < 2)
        {
            await SendCheerUpMessageAsync(ChatMessageType.OnlyOneFlew);
            return;
        }

        _log.Information("🏆 Publishing leaderboard for competition {CompetitionId} with {ResultCount} results", competition.Id, leaderboard.Count);

        await _mediator.Publish(new IntermediateCompetitionResult(leaderboard, competition));

        competition.ResultsPosted = true;
        await _competitions.SaveChangesAsync();
        _log.Debug("Marked leaderboard as published for competition {CompetitionId}", competition.Id);
    }

    public List<CompetitionResults> GetLocalLeaderboard(Competition competition)
    {
        return competition.TimeDeltas
            .GroupBy(d => d.PlayerName)
            .Select(d => d.MinBy(x => x.TrackTime))
            .OrderBy(d => d.TrackTime)
            .Select((x, i) => new CompetitionResults
            {
                CompetitionId = x.CompetitionId,
                PlayerName = x.PlayerName,
                UserId = x.UserId,
                TrackTime = x.TrackTime,
                LocalRank = i + 1,
                GlobalRank = x.Rank,
                Points = PointsByRank(i + 1),
                ModelName = x.ModelName
            })
            .ToList();
    }

    public async Task<List<SeasonResult>> GetSeasonResultsAsync(DateTime from, DateTime to)
    {
        _log.Debug("Calculating season results from {StartDate} to {EndDate}", from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));

        var results = await GetSeasonResultsQuery(from, to)
            .OrderByDescending(result => result.Points)
            .ToListAsync();

        for (var i = 0; i < results.Count; i++)
        {
            results[i].Rank = i + 1;
        }

        _log.Debug("Season results calculated: {ResultCount} pilots ranked", results.Count);
        return results;
    }

    public IQueryable<SeasonResult> GetSeasonResultsQuery(DateTime from, DateTime to)
    {
        return _competitions
            .GetAll(comp => comp.StartedOn >= from && comp.StartedOn <= to)
            .Where(comp => comp.State != CompetitionState.Cancelled)
            .SelectMany(comp => comp.CompetitionResults)
            .GroupBy(result => result.PlayerName)
            .Select(group => new SeasonResult
            {
                PlayerName = group.Key,
                Points = group.Sum(r => r.Points),
                GoldenCount = group.Count(r => r.LocalRank == 1),
                SilverCount = group.Count(r => r.LocalRank == 2),
                BronzeCount = group.Count(r => r.LocalRank == 3)
            });
    }

    private static int PointsByRank(int rank)
    {
        return rank switch
        {
            1 => 85,
            2 => 72,
            3 => 66,
            4 => 60,
            5 => 54,
            6 => 49,
            7 => 44,
            8 => 39,
            9 => 35,
            10 => 31,
            11 => 27,
            12 => 23,
            13 => 19,
            14 => 16,
            15 => 13,
            16 => 10,
            17 => 7,
            18 => 5,
            19 => 3,
            20 => 2,
            _ => 1
        };
    }

    private async Task SendCheerUpMessageAsync(ChatMessageType type)
    {
        if (DoNotDisturb(DateTime.Now))
        {
            _log.Debug("Skipping cheer-up message due to do-not-disturb hours (current time: {CurrentTime})", DateTime.Now.ToString("HH:mm"));
            return;
        }

        var cheerUpMessage = ChatMessages.GetRandomByTypeWithProbability(type);

        if (cheerUpMessage is null)
        {
            _log.Debug("No cheer-up message selected for type {MessageType}", type);
            return;
        }

        _log.Information("Sending cheer-up message of type {MessageType}", type);
        await _mediator.Publish(new CheerUp(cheerUpMessage));
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

    public async Task PublishDayStreakAchievements()
    {

        var streaks = new[] { 10, 20, 50, 75, 100, 150, 200, 250, 300, 365, 500, 1000 };

        var pilots = await _pilots
            .GetAll(p => streaks.Any(s => s == p.DayStreak))
            .ToListAsync();

        if (pilots.Count == 0)
        {
            _log.Debug("No pilots achieved milestone day streaks today");
            return;
        }

        _log.Information("Found {PilotCount} pilots with milestone day streaks: {PilotNames}",
            pilots.Count, string.Join(", ", pilots.Select(p => $"{p.Name} ({p.DayStreak})")));

        await _mediator.Publish(new DayStreakAchievements(pilots));

    }

    public async Task DayStreakPotentialLoseNotification()
    {

        var activeCompetition = await GetCurrentCompetitions()
            .FirstOrDefaultAsync();

        if (activeCompetition is null)
        {
            _log.Debug("No active competition found for day streak potential lose notification");
            return;
        }

        var leaderboard = GetLocalLeaderboard(activeCompetition)
            .Select(r => r.PlayerName)
            .ToArray();

        _log.Debug("Current leaderboard has {ParticipantCount} participants", leaderboard.Length);

        var pilots = await _pilots
            .GetAll(p => p.DayStreak > 10)
            .Where(p => leaderboard.All(l => l != p.Name))
            .ToListAsync();

        if (pilots.Count == 0)
        {
            _log.Debug("All pilots with significant day streaks have already participated today");
            return;
        }

        _log.Information("Found {PilotCount} pilots at risk of losing day streaks: {PilotNames}",
            pilots.Count, string.Join(", ", pilots.Select(p => $"{p.Name} ({p.DayStreak})")));

        await _mediator.Publish(new DayStreakPotentialLose(pilots));

    }
}
