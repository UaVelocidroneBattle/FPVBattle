using Telegram.Bot.Types;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Represents a Telegram bot instance configured for a specific channel/cup
/// </summary>
/// <remarks>
/// Provides channel-specific bot operations for sending messages, photos, and polls
/// to the appropriate Telegram channel based on cup configuration.
/// </remarks>
public interface ITelegramBotChannel
{
    /// <summary>
    /// Gets the channel ID this bot instance is configured for
    /// </summary>
    string ChannelId { get; }

    /// <summary>
    /// Sends a text message to the configured channel
    /// </summary>
    /// <param name="message">Message text (will be escaped for MarkdownV2)</param>
    Task SendMessageAsync(string message);

    /// <summary>
    /// Sends a reply to a specific message in a chat
    /// </summary>
    /// <param name="message">Reply text</param>
    /// <param name="messageId">ID of message to reply to</param>
    /// <param name="chatId">Target chat ID (defaults to channel if null)</param>
    /// <returns>Message ID of sent reply, or null if failed</returns>
    Task<int?> ReplyMessageAsync(string message, int messageId, string? chatId = null);

    /// <summary>
    /// Sends a photo from URL with optional caption
    /// </summary>
    Task SendPhotoAsync(string fileUrl, string? message = null);

    /// <summary>
    /// Sends a photo from stream with optional caption
    /// </summary>
    Task SendPhotoAsync(Stream file, string? message = null);

    /// <summary>
    /// Sends a poll to the channel
    /// </summary>
    /// <returns>Message ID of the poll, or null if failed</returns>
    Task<int?> SendPollAsync(BotPoll poll);

    /// <summary>
    /// Stops an active poll
    /// </summary>
    /// <param name="messageId">Poll message ID</param>
    /// <returns>Poll results, or null if failed</returns>
    Task<Poll?> StopPollAsync(int messageId);

    /// <summary>
    /// Removes a message from a chat
    /// </summary>
    Task RemoveMessageAsync(int messageId, string? chatId = null);

    /// <summary>
    /// Checks if the given chat ID matches this channel
    /// </summary>
    bool IsChannelId(string chatId);
}
