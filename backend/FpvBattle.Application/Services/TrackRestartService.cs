using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Notifications;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace FpvBattle.Application.Services;

public enum TrackRestartResult
{
    Restarted,
    RestartedMomentsAgo,
    CupNotEnabled
}

public class TrackRestartService
{
    private static readonly ILogger Log = Serilog.Log.ForContext<TrackRestartService>();
    private static readonly object CooldownLock = new();
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(5);

    private readonly IPublisher _publisher;
    private readonly IBackgroundJobClient _jobs;
    private readonly IMemoryCache _cache;
    private readonly ICupService _cupService;

    public TrackRestartService(
        IPublisher publisher,
        IBackgroundJobClient jobs,
        IMemoryCache cache,
        ICupService cupService)
    {
        _publisher = publisher;
        _jobs = jobs;
        _cache = cache;
        _cupService = cupService;
    }

    public async Task<TrackRestartResult> RestartAsync(string cupId, string? restartedBy)
    {
        if (!_cupService.GetEnabledCupIds().Contains(cupId))
        {
            Log.Warning("Track restart requested by {RestartedBy} in cup {CupId}, which is not an enabled cup", restartedBy, cupId);
            return TrackRestartResult.CupNotEnabled;
        }

        var cooldownKey = $"track-restart:{cupId}";

        lock (CooldownLock)
        {
            if (_cache.TryGetValue(cooldownKey, out _))
            {
                Log.Warning("Track restart requested by {RestartedBy} in cup {CupId} refused: the cup was restarted less than {Cooldown} ago", restartedBy, cupId, Cooldown);
                return TrackRestartResult.RestartedMomentsAgo;
            }

            _cache.Set(cooldownKey, true, Cooldown);
        }

        Log.Information("🔁 Track restart requested by {RestartedBy} in cup {CupId}", restartedBy ?? "an admin", cupId);

        _jobs.Enqueue<CompetitionConductor>(conductor => conductor.StartNewAsync(cupId));
        await _publisher.Publish(new TrackRestart(cupId, restartedBy));

        return TrackRestartResult.Restarted;
    }
}
