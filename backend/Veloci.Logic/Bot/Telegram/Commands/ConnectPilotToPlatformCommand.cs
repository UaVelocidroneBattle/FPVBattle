using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Services.Pilots;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class ConnectPilotToPlatformCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;
    private readonly PilotPlatformsService _pilotPlatformsService;

    public ConnectPilotToPlatformCommand(IRepository<Pilot> pilots, PilotPlatformsService pilotPlatformsService)
    {
        _pilots = pilots;
        _pilotPlatformsService = pilotPlatformsService;
    }

    public string[] Keywords => ["/connect"];
    public string Description => "/connect {platform} ({pilotName}) ({platformUsername})";
    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        // Admin command - works in any chat regardless of cup binding
        if (context.Parameters is null || context.Parameters.Length == 0)
            return "Use the command with this format: /connect {platform} ({pilotName}) ({platformUsername})";

        ParsedConnectCommand parsed;
        try
        {
            parsed = ParseParameters(context.Parameters);
        }
        catch (ArgumentException ex)
        {
            return $"Error: {ex.Message}";
        }

        var pilot = await _pilots.GetAll()
            .ByName(parsed.PilotName)
            .FirstOrDefaultAsync();

        if (pilot is null)
            return "Pilot not found";

        await _pilotPlatformsService.AddOrUpdatePlatformAsync(pilot, parsed.Platform, parsed.PlatformUsername);

        return $"Pilot '{parsed.PilotName}' successfully connected to platform '{parsed.Platform}' with username '{parsed.PlatformUsername}'.";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;

    private ParsedConnectCommand ParseParameters(string[] parameters)
    {
        if (parameters == null || parameters.Length < 3)
            throw new ArgumentException("Insufficient parameters provided.");

        // First parameter is always the platform
        var platformStr = parameters[0];

        if (!Enum.TryParse<PlatformNames>(platformStr, true, out var platform))
            throw new ArgumentException($"Unknown platform: {platformStr}");

        // Reconstruct bracketed values: pilotName and platformUsername
        var bracketedValues = CommandParseHelper.ReconstructBracketedValues(parameters.Skip(1).ToArray());

        if (bracketedValues.Count != 2)
            throw new ArgumentException("Expected two bracketed values: pilotName and platformUsername.");

        return new ParsedConnectCommand
        {
            Platform = platform,
            PilotName = bracketedValues[0],
            PlatformUsername = bracketedValues[1]
        };
    }
}

public class ParsedConnectCommand
{
    public PlatformNames Platform { get; set; }
    public string PilotName { get; set; }
    public string PlatformUsername { get; set; }
}

