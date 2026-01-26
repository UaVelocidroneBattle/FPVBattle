using Serilog;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Sends Discord messages to cups based on their configuration
/// </summary>
public class DiscordCupMessenger : IDiscordCupMessenger
{
    private static readonly ILogger _log = Log.ForContext<DiscordCupMessenger>();

    private readonly IDiscordBotFactory _botFactory;
    private readonly ICupService _cupService;

    public DiscordCupMessenger(IDiscordBotFactory botFactory, ICupService cupService)
    {
        _botFactory = botFactory;
        _cupService = cupService;
    }

    public Task SendMessageToCupsAsync(IEnumerable<string> cupIds, string message)
    {
        return SendToCupsAsync(cupIds, bot => bot.SendMessageAsync(message));
    }

    public Task SendMessageToCupAsync(string cupId, string message)
    {
        return SendMessageToCupsAsync(new[] { cupId }, message);
    }

    public Task SendMessageToAllCupsAsync(string message)
    {
        return SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    public Task SendImageToAllCupsAsync(byte[] image, string imageName)
    {
        return SendToAllCupsAsync(bot => bot.SendImageAsync(image, imageName));
    }

    private async Task SendToCupsAsync(IEnumerable<string> cupIds, Func<IDiscordBot, Task> sendAction)
    {
        foreach (var cupId in cupIds)
        {
            if (!_botFactory.TryGetBotForCup(cupId, out var bot) || bot is null)
            {
                _log.Warning("Cup {CupId} does not have Discord channel configured, skipping", cupId);
                continue;
            }

            try
            {
                await sendAction(bot);
                _log.Debug("Sent message to cup {CupId} Discord channel", cupId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to send message to cup {CupId} Discord channel", cupId);
            }
        }
    }

    private async Task SendToAllCupsAsync(Func<IDiscordBot, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();

        foreach (var cupId in enabledCupIds)
        {
            if (_botFactory.TryGetBotForCup(cupId, out var bot))
            {
                try
                {
                    await sendAction(bot);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Failed to send Discord message to cup {CupId}", cupId);
                }
            }
        }
    }
}
