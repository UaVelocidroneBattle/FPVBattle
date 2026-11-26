using FpvBattle.Application.Bot;
using FpvBattle.Application.Notifications;
using FpvBattle.Application.Persistence;
using FpvBattle.Application.Persistence.Queries;
using FpvBattle.Domain;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FpvBattle.Application.Services;

public class LeaderboardPublisher
{
    private static readonly ILogger Log = Serilog.Log.ForContext<LeaderboardPublisher>();

    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;
    private readonly ILeaderboardCalculator _leaderboardCalculator;

    public LeaderboardPublisher(
        IApplicationDbContext db,
        IPublisher publisher,
        ILeaderboardCalculator leaderboardCalculator)
    {
        _db = db;
        _publisher = publisher;
        _leaderboardCalculator = leaderboardCalculator;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [20, 120, 600])]
    [DisableConcurrentExecution("Competition", 1)]
    public async Task PublishCurrentLeaderboardAsync()
    {
        var activeCompetitions = await _db.Competitions.Active().NewestFirst().ToListAsync();

        if (!activeCompetitions.Any())
        {
            Log.Debug("No active competitions found for leaderboard publishing");
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
            Log.Debug("Leaderboard already published for competition {CompetitionId}, skipping", competition.Id);
            return;
        }

        if (competition.TimeDeltas.Count == 0)
        {
            Log.Information("No results yet for competition {CompetitionId}", competition.Id);
            await SendCheerUpMessageAsync(competition.CupId, ChatMessageType.NobodyFlying);
            return;
        }

        var leaderboard = _leaderboardCalculator.GetLeagueLeaderboard(competition);

        if (leaderboard.Sum(x => x.Results.Count) < 2)
        {
            await SendCheerUpMessageAsync(competition.CupId, ChatMessageType.OnlyOneFlew);
            return;
        }

        Log.Information("🏆 Publishing leaderboard for competition {CompetitionId} with {ResultCount} results", competition.Id, leaderboard.Count);

        await _publisher.Publish(new IntermediateCompetitionResult(leaderboard, competition));

        competition.ResultsPosted = true;
        await _db.SaveChangesAsync();
        Log.Debug("Marked leaderboard as published for competition {CompetitionId}", competition.Id);
    }

    private async Task SendCheerUpMessageAsync(string cupId, ChatMessageType type)
    {
        if (DoNotDisturb(DateTime.Now))
        {
            Log.Debug("Skipping cheer-up message due to do-not-disturb hours (current time: {CurrentTime})", DateTime.Now.ToString("HH:mm"));
            return;
        }

        await _publisher.Publish(new CheerUp(cupId, type));
    }

    private static bool DoNotDisturb(DateTime dateTime)
    {
        return dateTime.Hour is < 7 or > 22;
    }
}
