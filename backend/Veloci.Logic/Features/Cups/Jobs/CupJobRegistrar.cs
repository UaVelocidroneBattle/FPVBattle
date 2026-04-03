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
        _log.Information("Found {CupCount} enabled cups: {CupIds} (registering start, stop, stop-poll, season-results, and vote-reminder jobs)", enabledCupIds.Count, string.Join(", ", enabledCupIds));

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
        if (stopTime < TimeSpan.Zero)
            stopTime = stopTime.Add(TimeSpan.FromHours(24));

        var stopHour = stopTime.Hours;
        var stopMinute = stopTime.Minutes;

        // Stop poll time is 2 minutes before stop time
        var stopPollTime = stopTime.Add(TimeSpan.FromMinutes(-2));
        if (stopPollTime < TimeSpan.Zero)
            stopPollTime = stopPollTime.Add(TimeSpan.FromHours(24));

        var stopPollHour = stopPollTime.Hours;
        var stopPollMinute = stopPollTime.Minutes;

        _log.Debug("Cup {CupId} schedule - Start: {StartTime}, Stop poll: {StopPollTime}, Stop: {StopTime}",
            cupId,
            $"{startHour:D2}:{startMinute:D2}",
            $"{stopPollHour:D2}:{stopPollMinute:D2}",
            $"{stopHour:D2}:{stopMinute:D2}");

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

        // Register stop poll job
        var stopPollJobId = $"stop-poll-{cupId}";
        var stopPollCron = $"{stopPollMinute} {stopPollHour} * * *";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            stopPollJobId,
            conductor => conductor.StopPollAsync(cupId),
            stopPollCron,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}", stopPollJobId, stopPollCron, cupId);

        // Register season results job (1 minute before start)
        var seasonResultsTime = startTime.Add(TimeSpan.FromMinutes(-1));
        if (seasonResultsTime < TimeSpan.Zero)
            seasonResultsTime = seasonResultsTime.Add(TimeSpan.FromHours(24));

        var seasonResultsJobId = $"season-results-{cupId}";
        var seasonResultsCron = $"{seasonResultsTime.Minutes} {seasonResultsTime.Hours} * * *";
        RecurringJob.AddOrUpdate<CompetitionConductor>(
            seasonResultsJobId,
            conductor => conductor.SeasonResultsAsync(cupId),
            seasonResultsCron,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}", seasonResultsJobId, seasonResultsCron, cupId);

        // Register vote reminder job (if configured)
        if (!string.IsNullOrEmpty(cupOptions.Schedule.VoteReminderTime))
        {
            if (TimeSpan.TryParse(cupOptions.Schedule.VoteReminderTime, out var voteReminderTime))
            {
                var reminderJobId = $"vote-reminder-{cupId}";
                var reminderCron = $"{voteReminderTime.Minutes} {voteReminderTime.Hours} * * *";
                RecurringJob.AddOrUpdate<CompetitionConductor>(
                    reminderJobId,
                    conductor => conductor.VoteReminder(cupId),
                    reminderCron,
                    new RecurringJobOptions
                    {
                        TimeZone = TimeZoneInfo.Utc
                    });
                _log.Information("✅ Registered job {JobId} with cron '{Cron}' (UTC) for cup {CupId}",
                    reminderJobId, reminderCron, cupId);
            }
            else
            {
                _log.Error("Invalid VoteReminderTime format '{VoteReminderTime}' for cup {CupId}. Expected HH:mm format.",
                    cupOptions.Schedule.VoteReminderTime, cupId);
            }
        }
    }
}
