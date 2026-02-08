using Telegram.Bot.Types;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Sends Telegram messages to cups based on their configuration
/// </summary>
public interface ITelegramCupMessenger
{
    /// <summary>
    /// Sends a message to a specific cup that has Telegram configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="message">Message text</param>
    Task SendMessageToCupAsync(string cupId, string message);

    /// <summary>
    /// Sends a message to specific cups that have Telegram configured
    /// </summary>
    /// <param name="cupIds">Cup identifiers to send to</param>
    /// <param name="message">Message text</param>
    Task SendMessageToCupsAsync(IEnumerable<string> cupIds, string message);

    /// <summary>
    /// Sends a message to all enabled cups that have Telegram configured
    /// </summary>
    /// <param name="message">Message text</param>
    Task SendMessageToAllCupsAsync(string message);

    /// <summary>
    /// Sends a photo from URL to all enabled cups that have Telegram configured
    /// </summary>
    /// <param name="fileUrl">URL of the photo</param>
    /// <param name="caption">Optional caption</param>
    Task SendPhotoToAllCupsAsync(string fileUrl, string? caption = null);

    /// <summary>
    /// Sends a photo from stream to all enabled cups that have Telegram configured
    /// </summary>
    /// <param name="file">Photo stream</param>
    /// <param name="caption">Optional caption</param>
    Task SendPhotoToAllCupsAsync(Stream file, string? caption = null);

    /// <summary>
    /// Sends a photo from stream to a specific cup that has Telegram configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="file">Photo stream</param>
    /// <param name="caption">Optional caption</param>
    Task SendPhotoToCupAsync(string cupId, Stream file, string? caption = null);

    /// <summary>
    /// Sends a poll to a specific cup that has Telegram configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="poll">Poll to send</param>
    Task<int?> SendPollToCupAsync(string cupId, BotPoll poll);

    /// <summary>
    /// Stops an active poll in a specific cup
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="pollMessageId">Message ID of the poll to stop</param>
    Task<Poll?> StopPollInCupAsync(string cupId, int pollMessageId);

    /// <summary>
    /// Sends a reply to a specific message in a cup's Telegram channel
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="message">Message text</param>
    /// <param name="replyToMessageId">Message ID to reply to</param>
    Task<int?> SendReplyToCupAsync(string cupId, string message, int replyToMessageId);

    /// <summary>
    /// Removes a message from a cup's Telegram channel
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="messageId">Message ID to remove</param>
    Task RemoveMessageInCupAsync(string cupId, int messageId);
}
