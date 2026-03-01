using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram.Commands;

public class AddToWhitelistCommand : ITelegramCommand
{
    private readonly IWhiteListService _whitelist;

    public AddToWhitelistCommand(IWhiteListService whitelist)
    {
        _whitelist = whitelist;
    }

    public string[] Keywords => ["/whitelist"];
    public string Description => "`/whitelist {pilotName}`";
    public async Task<string> ExecuteAsync(TelegramCommandContext context)
    {
        if (context.Parameters is null || context.Parameters.Length == 0)
            return "все добре, але не вистачає імені пілота";

        var pilotName = string.Join(' ', context.Parameters);
        await _whitelist.AddToWhiteListAsync(pilotName);

        return $"{pilotName} whitelisted";
    }

    public bool RemoveMessageAfterDelay => false;
    public bool Public => false;
}
