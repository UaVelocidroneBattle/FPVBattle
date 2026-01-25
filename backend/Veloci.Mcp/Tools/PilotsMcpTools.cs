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
    private readonly IPilotProfileService _pilotProfileService;
    private readonly IMediator _mediator;

    public PilotsMcpTools(
        IRepository<Pilot> pilots,
        IPilotProfileService pilotProfileService,
        IMediator mediator)
    {
        _pilots = pilots;
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
