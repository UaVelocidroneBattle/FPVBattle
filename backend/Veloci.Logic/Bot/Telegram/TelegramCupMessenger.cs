using Serilog;
using Telegram.Bot.Types;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Sends Telegram messages to cups based on their configuration
/// </summary>
public class TelegramCupMessenger : ITelegramCupMessenger
{
    private static readonly ILogger _log = Log.ForContext<TelegramCupMessenger>();

    private readonly ICupService _cupService;
    private readonly ITelegramMessenger _messenger;

    public TelegramCupMessenger(ICupService cupService, ITelegramMessenger messenger)
    {
        _cupService = cupService;
        _messenger = messenger;
    }

    public Task SendMessageToCupAsync(string cupId, string message)
    {
        return SendMessageToCupsAsync([cupId], message);
    }

    public Task SendMessageToCupsAsync(IEnumerable<string> cupIds, string message)
    {
        return SendToCupsAsync(cupIds, channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public Task SendMessageToAllCupsAsync(string message)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public Task SendPhotoToAllCupsAsync(string fileUrl, string? caption = null)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, fileUrl, caption));
    }

    public Task SendPhotoToAllCupsAsync(Stream file, string? caption = null)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, file, caption));
    }

    public Task SendPhotoToCupAsync(string cupId, Stream file, string? caption = null)
    {
        return SendToCupsAsync([cupId], channelId => _messenger.SendPhotoAsync(channelId, file, caption));
    }

    public async Task<int?> SendPollToCupAsync(string cupId, BotPoll poll)
    {
        var channelId = _cupService.GetTelegramChannelId(cupId);

        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping poll creation", cupId);
            return null;
        }

        var pollId = await _messenger.SendPollAsync(channelId, poll);

        if (pollId == null) return null;

        _log.Debug("Sent poll to cup {CupId} Telegram channel {ChannelId}, message ID: {PollId}", cupId, channelId, pollId);
        return pollId;
    }

    public async Task<Poll?> StopPollInCupAsync(string cupId, int pollMessageId)
    {
        var channelId = _cupService.GetTelegramChannelId(cupId);

        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping poll stop", cupId);
            return null;
        }

        try
        {
            var poll = await _messenger.StopPollAsync(channelId, pollMessageId);
            _log.Debug("Stopped poll {PollId} in cup {CupId} Telegram channel {ChannelId}",
                pollMessageId, cupId, channelId);
            return poll;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to stop poll {PollId} in cup {CupId} Telegram channel",
                pollMessageId, cupId);
            return null;
        }
    }

    public async Task<int?> SendReplyToCupAsync(string cupId, string message, int replyToMessageId)
    {
        var channelId = _cupService.GetTelegramChannelId(cupId);

        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping reply", cupId);
            return null;
        }

        try
        {
            var messageId = await _messenger.SendMessageAsync(channelId, message, replyToMessageId);
            _log.Debug("Sent reply to message {ReplyToId} in cup {CupId} Telegram channel {ChannelId}, message ID: {MessageId}",
                replyToMessageId, cupId, channelId, messageId);
            return messageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send reply in cup {CupId} Telegram channel", cupId);
            return null;
        }
    }

    public async Task RemoveMessageInCupAsync(string cupId, int messageId)
    {
        var channelId = _cupService.GetTelegramChannelId(cupId);

        if (string.IsNullOrEmpty(channelId))
        {
            _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping message removal", cupId);
            return;
        }

        try
        {
            await _messenger.RemoveMessageAsync(channelId, messageId);
            _log.Debug("Removed message {MessageId} from cup {CupId} Telegram channel {ChannelId}",
                messageId, cupId, channelId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to remove message {MessageId} from cup {CupId} Telegram channel",
                messageId, cupId);
        }
    }

    private async Task SendToCupsAsync(IEnumerable<string> cupIds, Func<string, Task> sendAction)
    {
        foreach (var cupId in cupIds)
        {
            var channelId = _cupService.GetTelegramChannelId(cupId);

            if (string.IsNullOrEmpty(channelId))
            {
                _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping", cupId);
                continue;
            }

            try
            {
                await sendAction(channelId);
                _log.Debug("Sent message to cup {CupId} Telegram channel {ChannelId}", cupId, channelId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to send message to cup {CupId} Telegram channel", cupId);
            }
        }
    }

    private async Task SendToAllCupsAsync(Func<string, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();
        await SendToCupsAsync(enabledCupIds, sendAction);
    }
}
