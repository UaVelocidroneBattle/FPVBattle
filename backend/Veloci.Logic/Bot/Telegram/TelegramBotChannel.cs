using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Telegram bot instance configured for a specific channel
/// </summary>
public class TelegramBotChannel : ITelegramBotChannel
{
    private static readonly ILogger _log = Log.ForContext<TelegramBotChannel>();

    private readonly TelegramBotClient _client;
    private readonly string _channelId;

    public string ChannelId => _channelId;

    public TelegramBotChannel(TelegramBotClient client, string channelId)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            _log.Information("📲 Sending Telegram message to channel {ChannelId}: {MessagePreview}...",
                _channelId, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            await _client.SendTextMessageAsync(
                chatId: _channelId,
                text: Isolate(message),
                parseMode: ParseMode.MarkdownV2);

            _log.Debug("Telegram message sent successfully with {MessageLength} characters to {ChannelId}",
                message.Length, _channelId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}' to channel {ChannelId}", message, _channelId);
        }
    }

    public async Task<int?> ReplyMessageAsync(string message, int messageId, string? chatId = null)
    {
        var targetChatId = chatId ?? _channelId;

        try
        {
            _log.Information("Sending Telegram reply to message {MessageId} in chat {ChatId}: {MessagePreview}...",
                messageId, targetChatId, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            var result = await _client.SendTextMessageAsync(
                chatId: targetChatId,
                replyToMessageId: messageId,
                parseMode: ParseMode.MarkdownV2,
                text: Isolate(message));

            _log.Debug("Telegram reply sent successfully as message {ReplyMessageId}", result.MessageId);
            return result.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}' to chat {ChatId}", message, targetChatId);
            return null;
        }
    }

    public async Task SendPhotoAsync(string fileUrl, string? message = null)
    {
        if (message is not null)
            message = Isolate(message);

        try
        {
            _log.Information("🖼️ Sending Telegram photo from URL {PhotoUrl} to {ChannelId} with caption: {Caption}",
                fileUrl, _channelId, message ?? "(no caption)");

            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                caption: message,
                photo: new InputFileUrl(fileUrl)
            );

            _log.Debug("Telegram photo sent successfully as message {MessageId} to {ChannelId}",
                result.MessageId, _channelId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from URL {PhotoUrl} to {ChannelId}", fileUrl, _channelId);
        }
    }

    public async Task SendPhotoAsync(Stream file, string? message = null)
    {
        file.Position = 0; // Weird fix. It throws an exception without

        if (message is not null)
            message = Isolate(message);

        try
        {
            _log.Information("Sending Telegram photo from stream ({FileSize} bytes) to {ChannelId} with caption: {Caption}",
                file.Length, _channelId, message ?? "(no caption)");

            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                photo: new InputFileStream(file),
                caption: message
            );

            _log.Debug("Telegram photo from stream sent successfully as message {MessageId} to {ChannelId}",
                result.MessageId, _channelId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from stream to {ChannelId}", _channelId);
        }
    }

    public async Task<int?> SendPollAsync(BotPoll poll)
    {
        try
        {
            _log.Information("🗳️ Sending Telegram poll to {ChannelId}: {Question} with {OptionCount} options",
                _channelId, poll.Question, poll.Options.Count());

            var message = await _client.SendPollAsync(
                chatId: _channelId,
                question: poll.Question,
                options: poll.Options.Select(x => x.Text)
            );

            _log.Information("Telegram poll sent successfully as message {MessageId} to {ChannelId}",
                message.MessageId, _channelId);
            return message.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a poll with question '{Question}' to {ChannelId}",
                poll.Question, _channelId);
            return null;
        }
    }

    public async Task<Poll?> StopPollAsync(int messageId)
    {
        try
        {
            _log.Information("Stopping Telegram poll with message ID {MessageId} in {ChannelId}", messageId, _channelId);
            var result = await _client.StopPollAsync(_channelId, messageId);

            if (result != null)
            {
                _log.Information("Telegram poll {MessageId} stopped successfully with {VoterCount} total votes in {ChannelId}",
                    messageId, result.TotalVoterCount, _channelId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to stop the poll with message ID {MessageId} in {ChannelId}",
                messageId, _channelId);
            return null;
        }
    }

    public async Task RemoveMessageAsync(int messageId, string? chatId = null)
    {
        var targetChatId = chatId ?? _channelId;

        try
        {
            _log.Debug("Removing Telegram message {MessageId} from chat {ChatId}", messageId, targetChatId);
            await _client.DeleteMessageAsync(targetChatId, messageId);
            _log.Debug("Telegram message {MessageId} removed successfully from {ChatId}", messageId, targetChatId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to remove message {MessageId} from chat {ChatId}", messageId, targetChatId);
        }
    }

    public bool IsChannelId(string chatId)
    {
        return chatId == _channelId;
    }

    private static string Isolate(string message) => message
        .Replace(".", "\\.")
        .Replace("!", "\\!")
        .Replace("-", "\\-")
        .Replace("_", "\\_")
        .Replace(")", "\\)")
        .Replace("(", "\\(")
        .Replace("#", "\\#");
}
