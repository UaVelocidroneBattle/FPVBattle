using Hangfire;
using Serilog;
using Veloci.Logic.Jobs;
using Veloci.Logic.Services;

namespace Veloci.Logic.Features.Cups.Jobs;

/// <summary>
/// Registers Hangfire recurring jobs dynamically for each enabled cup
/// </summary>
public class CupJobRegistrar : IJobRegistrar
{
    private static readonly ILogger _log = Log.ForContext<CupJobRegistrar>();

    private readonly ICupService _cupService;

    public CupJobRegistrar(ICupService cupService)
    {
        _cupService = cupService;
    }

    public void RegisterJobs()
    {
        _log.Information("Starting dynamic cup job registration");

        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();
        _log.Information("Found {CupCount} enabled cups: {CupIds}", enabledCupIds.Count, string.Join(", ", enabledCupIds));

        foreach (var cupId in enabledCupIds)
        {
            RegisterCupJobs(cupId);
        }

        _log.Information("Cup job registration completed for {CupCount} cups", enabledCupIds.Count);
    }

    private void RegisterCupJobs(string cupId)
    {
        var cupOptions = _cupService.GetCupOptions(cupId);

        if (!cupOptions.IsEnabled)
        {
            _log.Warning("Skipping job registration for disabled cup {CupId}", cupId);
            return;
        }

        _log.Information("Registering jobs for cup {CupId} ({CupName})", cupId, cupOptions.Name);

        // Parse start time (format: "HH:mm")
        if (!TimeSpan.TryParse(cupOptions.Schedule.StartTime, out var startTime))
        {
            _log.Error("Invalid StartTime format '{StartTime}' for cup {CupId}. Expected HH:mm format.", cupOptions.Schedule.StartTime, cupId);
            return;
        }

        var startHour = startTime.Hours;
        var startMinute = startTime.Minutes;

        // Stop time is 2 minutes before start time
        var stopTime = startTime.Add(TimeSpan.FromMinutes(-2));
        var stopHour = stopTime.Hours;
        var stopMinute = stopTime.Minutes;

        // If stop time wraps to previous day, adjust
        if (stopTime < TimeSpan.Zero)
        {
            stopTime = stopTime.Add(TimeSpan.FromHours(24));
            stopHour = stopTime.Hours;
            stopMinute = stopTime.Minutes;
        }

        _log.Debug("Cup {CupId} schedule - Start: {StartTime}, Stop: {StopTime}", cupId, $"{startHour:D2}:{startMinute:D2}", $"{stopHour:D2}:{stopMinute:D2}");

        // Register start competition job
        var startJobId = $"start-competition-{cupId}";
        var startCron = $"{startMinute} {startHour} * * *";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            startJobId,
            conductor => conductor.StartNewAsync(cupId),
            startCron,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}", startJobId, startCron, cupId);

        // Register stop competition job
        var stopJobId = $"stop-competition-{cupId}";
        var stopCron = $"{stopMinute} {stopHour} * * *";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            stopJobId,
            conductor => conductor.StopAsync(cupId),
            stopCron,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}", stopJobId, stopCron, cupId);

        // Register stop poll job (runs 10 seconds before stop)
        var stopPollJobId = $"stop-poll-{cupId}";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            stopPollJobId,
            conductor => conductor.StopPollAsync(cupId),
            stopCron,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}", stopPollJobId, stopCron, cupId);
    }
}
