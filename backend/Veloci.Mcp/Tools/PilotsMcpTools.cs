using System.ComponentModel;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Notifications;
using Veloci.Logic.Services.Pilots;
using Veloci.Logic.Services.Pilots.Models;

namespace Veloci.Mcp.Tools;

[McpServerToolType]
public class PilotsMcpTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly ILogger Log = Serilog.Log.ForContext<PilotsMcpTools>();
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<TrackTimeDelta> _deltas;
    private readonly IPilotProfileService _pilotProfileService;
    private readonly IMediator _mediator;

    public PilotsMcpTools(
        IRepository<Pilot> pilots,
        IRepository<TrackTimeDelta> deltas,
        IPilotProfileService pilotProfileService,
        IMediator mediator)
    {
        _pilots = pilots;
        _deltas = deltas;
        _pilotProfileService = pilotProfileService;
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Get a list of all registered pilots in the FPV Battle system.")]
    public async Task<string> GetAllPilots()
    {
        Log.Information("Getting all pilots");

        var result = await _pilots
            .GetAll()
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Name,
                p.DayStreak,
                p.MaxDayStreak,
                p.LastRaceDate
            })
            .ToListAsync();

        Log.Information("Retrieved {Count} pilots", result.Count);

        return JsonSerializer.Serialize(result, JsonOptions);
    }

    [McpServerTool]
    [Description("Get detailed profile information for a specific pilot by name, including statistics and achievements")]
    public async Task<PilotProfileModel> GetPilotProfile([Description("The name of the pilot")] string pilotName)
    {
        Log.Information("Getting profile for pilot {PilotName}", pilotName);

        if (string.IsNullOrWhiteSpace(pilotName))
        {
            throw new ArgumentException("'pilotName' cannot be null or empty.");
        }

        var profile = await _pilotProfileService.GetPilotProfileAsync(pilotName, CancellationToken.None);

        Log.Information("Retrieved profile for pilot {PilotName}", pilotName);

        return profile;
    }

    [McpServerTool]
    [Description("Recalculate the current day streak for a pilot based on their actual delta history. Walks back from yesterday until a day with no activity is found. Returns the recalculated value without saving it.")]
    public async Task<string> RecalculateDayStreak(
        [Description("The name of the pilot")] string pilotName)
    {
        Log.Information("Recalculating day streak for pilot {PilotName}", pilotName);

        if (string.IsNullOrEmpty(pilotName))
            throw new ArgumentException("'pilotName' cannot be null or empty.");

        var pilot = await _pilots
            .GetAll()
            .ByName(pilotName)
            .FirstOrDefaultAsync();

        if (pilot is null)
        {
            Log.Warning("Pilot {PilotName} not found", pilotName);
            throw new Exception("Pilot not found");
        }

        var deltaDates = await _deltas.GetAll()
            .Where(d => d.PilotId == pilot.Id)
            .Select(d => d.Date)
            .ToListAsync();

        // Shift each date back by 2 minutes to absorb the midnight race condition:
        // deltas created at ~00:00 UTC belong to the previous competition day.
        var daysWithActivity = deltaDates
            .Select(d => d.AddMinutes(-2).Date)
            .ToHashSet();

        var streak = 0;
        var day = DateTime.UtcNow.Date.AddDays(-1);

        while (daysWithActivity.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }

        Log.Information("Recalculated day streak for {PilotName}: {Streak} (stored: {Stored})", pilotName, streak, pilot.DayStreak);

        return $"Recalculated streak: {streak} (currently stored: {pilot.DayStreak})";
    }

    [McpServerTool]
    [Description("Set the day streak value for a pilot. Use to correct streak after a data issue.")]
    public async Task<string> SetDayStreak(
        [Description("The name of the pilot")] string pilotName,
        [Description("The day streak value to set")] int dayStreak)
    {
        Log.Information("Setting day streak for pilot {PilotName} to {DayStreak}", pilotName, dayStreak);

        if (string.IsNullOrEmpty(pilotName))
            throw new ArgumentException("'pilotName' cannot be null or empty.");

        if (dayStreak < 0)
            throw new ArgumentException("'dayStreak' cannot be negative.");

        var pilot = await _pilots
            .GetAll()
            .ByName(pilotName)
            .FirstOrDefaultAsync();

        if (pilot is null)
        {
            Log.Warning("Pilot {PilotName} not found", pilotName);
            throw new Exception("Pilot not found");
        }

        var previous = pilot.DayStreak;
        pilot.DayStreak = dayStreak;

        if (dayStreak > pilot.MaxDayStreak)
            pilot.MaxDayStreak = dayStreak;

        await _pilots.SaveChangesAsync();

        Log.Information("Day streak for pilot {PilotName} updated from {Previous} to {DayStreak}", pilotName, previous, dayStreak);

        return $"Day streak for {pilot.Name} updated from {previous} to {dayStreak}";
    }

    [McpServerTool]
    [Description("Add a day streak freeze for a pilot. Optionally sends a notification.")]
    public async Task<string> AddFreezie(
        [Description("The name of the pilot")] string pilotName,
        [Description("If true, sends a notification that a freezie was added")] bool notify = false)
    {
        Log.Information("Adding freezie for pilot {PilotName}, notify: {Notify}", pilotName, notify);

        if (string.IsNullOrEmpty(pilotName))
        {
            throw new ArgumentException("'pilotName' cannot be null or empty.");
        }

        var pilot = await _pilots
            .GetAll()
            .ByName(pilotName)
            .FirstOrDefaultAsync();

        if (pilot is null)
        {
            Log.Warning("Pilot {PilotName} not found", pilotName);
            throw new Exception("Pilot not found");
        }

        var today = DateTime.Today;
        pilot.DayStreakFreezes.Add(new DayStreakFreeze(today));
        await _pilots.SaveChangesAsync();

        Log.Information("Freezie added for pilot {PilotName}", pilotName);

        if (notify)
        {
            await _mediator.Publish(new FreezieAdded(pilotName));
            Log.Information("Notification sent for pilot {PilotName}", pilotName);
        }

        return "Freezie successfully added";
    }
}
