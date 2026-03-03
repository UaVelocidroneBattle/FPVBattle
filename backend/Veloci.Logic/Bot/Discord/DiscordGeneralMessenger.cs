using Serilog;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Sends Discord messages to the general channel
/// </summary>
public class DiscordGeneralMessenger : IDiscordGeneralMessenger
{
    private static readonly ILogger _log = Log.ForContext<DiscordGeneralMessenger>();

    private readonly IDiscordBotFactory _botFactory;

    public DiscordGeneralMessenger(IDiscordBotFactory botFactory)
    {
        _botFactory = botFactory;
    }

    public async Task SendMessageAsync(string message)
    {
        if (!_botFactory.TryGetGeneralBot(out var bot))
        {
            _log.Warning("Discord general channel is not configured, skipping message");
            return;
        }

        await bot.SendMessageAsync(message);
    }
}
