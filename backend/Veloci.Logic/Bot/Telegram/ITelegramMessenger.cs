using Telegram.Bot.Types;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Thin messenger interface for sending Telegram messages.
/// Callers are responsible for resolving cupId to channelId via ICupService.
/// </summary>
public interface ITelegramMessenger
{
    /// <summary>
    /// Whether Telegram is configured and available
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Sends a text message to the specified chat
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="message">Message text (will be escaped for MarkdownV2)</param>
    Task SendMessageAsync(string chatId, string message);

    /// <summary>
    /// Sends a reply to a specific message in a chat
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="message">Reply text (will be escaped for MarkdownV2)</param>
    /// <param name="replyToMessageId">ID of message to reply to</param>
    /// <returns>Message ID of sent reply, or null if failed</returns>
    Task<int?> SendMessageAsync(string chatId, string message, int replyToMessageId);

    /// <summary>
    /// Sends a photo from URL with optional caption
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="fileUrl">URL of the photo</param>
    /// <param name="caption">Optional caption</param>
    Task SendPhotoAsync(string chatId, string fileUrl, string? caption = null);

    /// <summary>
    /// Sends a photo from stream with optional caption
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="file">Photo stream</param>
    /// <param name="caption">Optional caption</param>
    Task SendPhotoAsync(string chatId, Stream file, string? caption = null);

    /// <summary>
    /// Sends a poll to the specified chat
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="poll">Poll configuration</param>
    /// <returns>Message ID of the poll, or null if failed</returns>
    Task<int?> SendPollAsync(string chatId, BotPoll poll);

    /// <summary>
    /// Stops an active poll
    /// </summary>
    /// <param name="chatId">Chat containing the poll</param>
    /// <param name="messageId">Poll message ID</param>
    /// <returns>Poll results, or null if failed</returns>
    Task<Poll?> StopPollAsync(string chatId, int messageId);

    /// <summary>
    /// Removes a message from a chat
    /// </summary>
    /// <param name="chatId">Target chat ID</param>
    /// <param name="messageId">Message ID to remove</param>
    Task RemoveMessageAsync(string chatId, int messageId);
}
