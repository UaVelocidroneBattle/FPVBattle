using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Serilog;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Services;

namespace Veloci.Mcp.Tools;

[McpServerToolType]
public class WhiteListMcpTools
{
    private static readonly ILogger Log = Serilog.Log.ForContext<WhiteListMcpTools>();
    private readonly IRepository<WhiteListedPilot> _whitelist;
    private readonly WhiteListService _whiteListService;

    public WhiteListMcpTools(IRepository<WhiteListedPilot> whitelist, WhiteListService whiteListService)
    {
        _whitelist = whitelist;
        _whiteListService = whiteListService;
    }

    [McpServerTool]
    [Description("Get a list of all pilots currently on the whitelist.")]
    public async Task<List<string>> GetWhiteList()
    {
        Log.Information("Fetching whitelist");

        var pilots = await _whitelist
            .GetAll()
            .OrderBy(p => p.PilotName)
            .Select(p => p.PilotName)
            .ToListAsync();

        Log.Information("Whitelist contains {Count} pilots", pilots.Count);

        return pilots;
    }

    [McpServerTool]
    [Description("Add a pilot to the whitelist by name.")]
    public async Task<string> AddToWhiteList([Description("The name of the pilot to whitelist")] string pilotName)
    {
        Log.Information("Adding pilot {PilotName} to whitelist", pilotName);

        if (string.IsNullOrWhiteSpace(pilotName))
            throw new ArgumentException("'pilotName' cannot be null or empty.");

        await _whiteListService.AddToWhiteListAsync(pilotName);

        Log.Information("Pilot {PilotName} added to whitelist", pilotName);

        return $"Pilot '{pilotName}' successfully added to whitelist";
    }

    [McpServerTool]
    [Description("Remove a pilot from the whitelist by name.")]
    public async Task<string> RemoveFromWhiteList([Description("The name of the pilot to remove")] string pilotName)
    {
        Log.Information("Removing pilot {PilotName} from whitelist", pilotName);

        if (string.IsNullOrWhiteSpace(pilotName))
            throw new ArgumentException("'pilotName' cannot be null or empty.");

        await _whiteListService.RemoveFromWhiteListAsync(pilotName);

        Log.Information("Pilot {PilotName} removed from whitelist", pilotName);

        return $"Pilot '{pilotName}' successfully removed from whitelist";
    }
}