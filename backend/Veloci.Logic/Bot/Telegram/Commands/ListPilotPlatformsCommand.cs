using System.Text;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class ListPilotPlatformsCommand : ITelegramCommand
{
    private readonly IRepository<Pilot> _pilots;

    public ListPilotPlatformsCommand(IRepository<Pilot> pilots)
    {
        _pilots = pilots;
    }

    public string[] Keywords => ["/list-pilot-platforms"];
    public string Description => "/list-pilot-platforms";
    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        // Admin command - works in any chat regardless of cup binding
        var pilots = _pilots
            .GetAll(p => p.PlatformAccounts.Any())
            .ToList();

        if (pilots.Count == 0)
            return "No pilots with linked platforms found.";

        var result = new StringBuilder();

        foreach (var pilot in pilots)
        {
            result.AppendLine($"*{pilot.Name}*:");

            foreach (var account in pilot.PlatformAccounts)
            {
                result.AppendLine($" - {account.PlatformName}: {account.Username}");
            }

            result.AppendLine();
        }

        return result.ToString();
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;
}
