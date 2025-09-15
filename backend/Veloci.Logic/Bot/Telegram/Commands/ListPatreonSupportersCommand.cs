using System.Text;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Bot.Telegram.Commands.Core;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class ListPatreonSupportersCommand : ITelegramCommand
{
    private readonly IRepository<PatreonSupporter> _supporters;

    public ListPatreonSupportersCommand(IRepository<PatreonSupporter> supporters)
    {
        _supporters = supporters;
    }

    public string[] Keywords => ["/list-patreon"];
    public string Description => "/list-patreon";
    public async Task<string> ExecuteAsync(string[]? parameters)
    {
        var supporters = _supporters.GetAll().ToList();

        if (supporters.Count == 0)
            return "No Patreon supporters found.";

        var result = new StringBuilder();

        foreach (var supporter in supporters)
        {
            var pilotName = supporter.Pilot?.Name ?? "Unlinked";
            result.AppendLine($"*{supporter.Name}* ({supporter.TierName}) / Pilot: *{pilotName}*");
        }

        return result.ToString();
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;
}
