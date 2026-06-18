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
        return SendMessageToCupsAsync([cupId], message);
    }

    public Task SendMessageToAllCupsAsync(string message)
    {
        return SendToAllCupsAsync(bot => bot.SendMessageAsync(message));
    }

    public Task SendImageToAllCupsAsync(byte[] image, string imageName)
    {
        return SendToAllCupsAsync(bot => bot.SendImageAsync(image, imageName));
    }

    public Task SendImageToCupAsync(string cupId, byte[] image, string imageName)
    {
        return SendToCupsAsync([cupId], bot => bot.SendImageAsync(image, imageName));
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

    public async Task<ulong?> SendPollToCupAsync(string cupId, BotPoll poll)
    {
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("Cup {CupId} does not have Discord channel configured, skipping poll creation", cupId);
            return null;
        }

        try
        {
            var pollMessageId = await bot.SendPollAsync(poll);
            _log.Debug("Sent poll to cup {CupId} Discord channel, message ID: {PollId}", cupId, pollMessageId);
            return pollMessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send poll to cup {CupId} Discord channel", cupId);
            return null;
        }
    }

    public async Task<DiscordPollResult?> StopPollInCupAsync(string cupId, ulong pollMessageId)
    {
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("Cup {CupId} does not have Discord channel configured, skipping poll stop", cupId);
            return null;
        }

        try
        {
            var result = await bot.StopPollAsync(pollMessageId);
            _log.Debug("Stopped poll {PollId} in cup {CupId} Discord channel", pollMessageId, cupId);
            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to stop poll {PollId} in cup {CupId} Discord channel", pollMessageId, cupId);
            return null;
        }
    }

    public async Task<ulong?> SendReplyToCupAsync(string cupId, string message, ulong replyToMessageId)
    {
        if (!_botFactory.TryGetBotForCup(cupId, out var bot))
        {
            _log.Warning("Cup {CupId} does not have Discord channel configured, skipping reply", cupId);
            return null;
        }

        try
        {
            var messageId = await bot.SendReplyAsync(message, replyToMessageId);
            _log.Debug("Sent reply to message {ReplyToId} in cup {CupId} Discord channel, message ID: {MessageId}",
                replyToMessageId, cupId, messageId);
            return messageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send reply in cup {CupId} Discord channel", cupId);
            return null;
        }
    }
}
