using System.Text;
using FpvBattle.Domain;
using FpvBattle.Logic.Bot.Telegram.Commands.Core;
using FpvBattle.Logic.Persistence;

namespace FpvBattle.Logic.Bot.Telegram.Commands;

public class ListPilotPlatformsCommand : ITelegramCommand
{
    private readonly IApplicationDbContext _db;

    public ListPilotPlatformsCommand(IApplicationDbContext db)
    {
        _db = db;
    }

    public string[] Keywords => ["/list-pilot-platforms"];
    public string Description => "/list-pilot-platforms";
    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        // Admin command - works in any chat regardless of cup binding
        var pilots = _db.Pilots.Where(p => p.PlatformAccounts.Any())
            .ToList();

        if (pilots.Count == 0)
            return "No pilots with linked platforms found.";

        var result = new StringBuilder();

        foreach (var pilot in pilots)
        {
            result.AppendLine($"*{TelegramMarkdown.Escape(pilot.Name)}*:");

            foreach (var account in pilot.PlatformAccounts)
            {
                result.AppendLine($" - {account.PlatformName}: {TelegramMarkdown.Escape(account.Username)}");
            }

            result.AppendLine();
        }

        return result.ToString();
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;
}
