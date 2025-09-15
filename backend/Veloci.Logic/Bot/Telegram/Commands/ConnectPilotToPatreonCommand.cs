using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class ConnectPilotToPatreonCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<PatreonSupporter> _supporters;

    public ConnectPilotToPatreonCommand(IRepository<Pilot> pilots, IRepository<PatreonSupporter> supporters)
    {
        _pilots = pilots;
        _supporters = supporters;
    }

    public string[] Keywords => ["/connect-patreon"];
    public string Description => "/connect-patreon ({pilotName}) ({patreonUsername})";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        if (parameters is null || parameters.Length == 0)
            return "Use the command with this format: /connect-patreon ({pilotName}) ({patreonUsername})";

        ParsedConnectCommand parsed;
        try
        {
            parsed = ParseParameters(parameters);
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

        var supporter = await _supporters.GetAll()
            .FirstOrDefaultAsync(x => x.Name == parsed.PlatformUsername);

        if (supporter is null)
            return "Patreon supporter not found";

        supporter.PilotId = pilot.Id;
        await _supporters.SaveChangesAsync();

        return $"Pilot '{parsed.PilotName}' successfully connected to Patreon supporter '{parsed.PlatformUsername}'.";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;

    private ParsedConnectCommand ParseParameters(string[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
            throw new ArgumentException("Insufficient parameters provided.");

        // Reconstruct bracketed values: pilotName and platformUsername
        var bracketedValues = CommandParseHelper.ReconstructBracketedValues(parameters);

        if (bracketedValues.Count != 2)
            throw new ArgumentException("Expected two bracketed values: pilotName and patreonUserName.");

        return new ParsedConnectCommand
        {
            PilotName = bracketedValues[0],
            PlatformUsername = bracketedValues[1]
        };
    }
}
