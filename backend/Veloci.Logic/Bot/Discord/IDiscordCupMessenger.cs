namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Sends Discord messages to cups based on their configuration
/// </summary>
public interface IDiscordCupMessenger
{
    /// <summary>
    /// Sends a message to a specific cup that has Discord configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="message">Message text</param>
    Task SendMessageToCupAsync(string cupId, string message);

    /// <summary>
    /// Sends a message to specific cups that have Discord configured
    /// </summary>
    /// <param name="cupIds">Cup identifiers to send to</param>
    /// <param name="message">Message text</param>
    Task SendMessageToCupsAsync(IEnumerable<string> cupIds, string message);

    /// <summary>
    /// Sends a message to all enabled cups that have Discord configured
    /// </summary>
    /// <param name="message">Message text</param>
    Task SendMessageToAllCupsAsync(string message);

    /// <summary>
    /// Sends an image to all enabled cups that have Discord configured
    /// </summary>
    /// <param name="image">Image bytes</param>
    /// <param name="imageName">Image file name</param>
    Task SendImageToAllCupsAsync(byte[] image, string imageName);

    /// <summary>
    /// Sends an image to a specific cup that has Discord configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="image">Image bytes</param>
    /// <param name="imageName">Image file name</param>
    Task SendImageToCupAsync(string cupId, byte[] image, string imageName);
    
    /// <summary>
    /// Sends a poll to a specific cup that has Discord configured
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="poll">Poll to send</param>
    /// <returns>Message ID of the poll, or null if failed</returns>
    Task<ulong?> SendPollToCupAsync(string cupId, BotPoll poll);

    /// <summary>
    /// Stops an active poll in a specific cup and returns results
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="pollMessageId">Message ID of the poll to stop</param>
    /// <returns>Poll results, or null if failed</returns>
    Task<DiscordPollResult?> StopPollInCupAsync(string cupId, ulong pollMessageId);

    /// <summary>
    /// Sends a reply to a specific message in a cup's Discord channel
    /// </summary>
    /// <param name="cupId">Cup identifier to send to</param>
    /// <param name="message">Message text</param>
    /// <param name="replyToMessageId">Message ID to reply to</param>
    /// <returns>Message ID of the reply, or null if failed</returns>
    Task<ulong?> SendReplyToCupAsync(string cupId, string message, ulong replyToMessageId);
}
