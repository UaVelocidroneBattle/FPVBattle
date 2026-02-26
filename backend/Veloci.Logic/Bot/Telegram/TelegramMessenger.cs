using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Implementation of ITelegramMessenger that sends messages via the shared ITelegramBotClient.
/// </summary>
public class TelegramMessenger : ITelegramMessenger
{
    private static readonly ILogger _log = Log.ForContext<TelegramMessenger>();

    private readonly ITelegramBotClient _client;

    public bool IsAvailable => true;

    public TelegramMessenger(ITelegramBotClient client)
    {
        _client = client;
    }

    public async Task SendMessageAsync(string chatId, string message)
    {
        try
        {
            _log.Information("Sending Telegram message to chat {ChatId}: {MessagePreview}...",
                chatId, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            await _client.SendTextMessageAsync(
                chatId: chatId,
                text: EscapeMarkdownV2(message),
                parseMode: ParseMode.MarkdownV2);

            _log.Debug("Telegram message sent successfully with {MessageLength} characters to {ChatId}",
                message.Length, chatId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}' to chat {ChatId}", message, chatId);
        }
    }

    public async Task<int?> SendMessageAsync(string chatId, string message, int replyToMessageId)
    {
        try
        {
            _log.Information("Sending Telegram reply to message {MessageId} in chat {ChatId}: {MessagePreview}...",
                replyToMessageId, chatId, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            var result = await _client.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: replyToMessageId,
                parseMode: ParseMode.MarkdownV2,
                text: EscapeMarkdownV2(message));

            _log.Debug("Telegram reply sent successfully as message {ReplyMessageId}", result.MessageId);
            return result.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}' to chat {ChatId}", message, chatId);
            return null;
        }
    }

    public async Task SendPhotoAsync(string chatId, string fileUrl, string? caption = null)
    {
        if (!IsAvailable)
        {
            _log.Debug("Telegram not configured, skipping photo send to {ChatId}", chatId);
            return;
        }

        var escapedCaption = caption is not null ? EscapeMarkdownV2(caption) : null;

        try
        {
            _log.Information("Sending Telegram photo from URL {PhotoUrl} to {ChatId} with caption: {Caption}",
                fileUrl, chatId, caption ?? "(no caption)");

            var result = await _client.SendPhotoAsync(
                chatId: chatId,
                caption: escapedCaption,
                photo: new InputFileUrl(fileUrl));

            _log.Debug("Telegram photo sent successfully as message {MessageId} to {ChatId}",
                result.MessageId, chatId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from URL {PhotoUrl} to {ChatId}", fileUrl, chatId);
        }
    }

    public async Task SendPhotoAsync(string chatId, Stream file, string? caption = null)
    {
        if (!IsAvailable)
        {
            _log.Debug("Telegram not configured, skipping photo stream send to {ChatId}", chatId);
            return;
        }

        file.Position = 0; // Weird fix. It throws an exception without

        var escapedCaption = caption is not null ? EscapeMarkdownV2(caption) : null;

        try
        {
            _log.Information("Sending Telegram photo from stream ({FileSize} bytes) to {ChatId} with caption: {Caption}",
                file.Length, chatId, caption ?? "(no caption)");

            var result = await _client.SendPhotoAsync(
                chatId: chatId,
                photo: new InputFileStream(file),
                caption: escapedCaption);

            _log.Debug("Telegram photo from stream sent successfully as message {MessageId} to {ChatId}",
                result.MessageId, chatId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from stream to {ChatId}", chatId);
        }
    }

    public async Task<int?> SendPollAsync(string chatId, BotPoll poll)
    {
        if (!IsAvailable)
        {
            _log.Debug("Telegram not configured, skipping poll send to {ChatId}", chatId);
            return null;
        }

        try
        {
            _log.Information("Sending Telegram poll to {ChatId}: {Question} with {OptionCount} options",
                chatId, poll.Question, poll.Options.Count());

            var message = await _client.SendPollAsync(
                chatId: chatId,
                question: poll.Question,
                options: poll.Options.Select(x => x.Text));

            _log.Information("Telegram poll sent successfully as message {MessageId} to {ChatId}",
                message.MessageId, chatId);
            return message.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a poll with question '{Question}' to {ChatId}",
                poll.Question, chatId);
            return null;
        }
    }

    public async Task<Poll?> StopPollAsync(string chatId, int messageId)
    {
        if (!IsAvailable)
        {
            _log.Debug("Telegram not configured, skipping poll stop in {ChatId}", chatId);
            return null;
        }

        try
        {
            _log.Information("Stopping Telegram poll with message ID {MessageId} in {ChatId}", messageId, chatId);
            var result = await _client.StopPollAsync(chatId, messageId);

            if (result != null)
            {
                _log.Information("Telegram poll {MessageId} stopped successfully with {VoterCount} total votes in {ChatId}",
                    messageId, result.TotalVoterCount, chatId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to stop the poll with message ID {MessageId} in {ChatId}",
                messageId, chatId);
            return null;
        }
    }

    public async Task RemoveMessageAsync(string chatId, int messageId)
    {
        if (!IsAvailable)
        {
            _log.Debug("Telegram not configured, skipping message removal in {ChatId}", chatId);
            return;
        }

        try
        {
            _log.Debug("Removing Telegram message {MessageId} from chat {ChatId}", messageId, chatId);
            await _client.DeleteMessageAsync(chatId, messageId);
            _log.Debug("Telegram message {MessageId} removed successfully from {ChatId}", messageId, chatId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to remove message {MessageId} from chat {ChatId}", messageId, chatId);
        }
    }

    /// <summary>
    /// Escapes special characters for Telegram MarkdownV2 format
    /// </summary>
    private static string EscapeMarkdownV2(string message) => message
        .Replace(".", "\\.")
        .Replace("!", "\\!")
        .Replace("-", "\\-")
        .Replace("_", "\\_")
        .Replace(")", "\\)")
        .Replace("(", "\\(")
        .Replace("#", "\\#");
}
