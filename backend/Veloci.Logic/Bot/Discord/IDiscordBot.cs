namespace Veloci.Logic.Bot.Discord;

public interface IDiscordBot
{
    Task<ulong?> SendMessageAsync(string message);
    Task EditMessageAsync(ulong messageId, string message);
    Task SendMessageInThreadAsync(ulong messageId, string threadName, string message);
    Task ArchiveThreadAsync(string threadName);
    Task ChangeChannelTopicAsync(string message);
    Task SendImageAsync(byte[] imageBytes, string imageName);
    
    /// <summary>
    /// Sends a poll to the Discord channel
    /// </summary>
    /// <param name="poll">Poll configuration</param>
    /// <returns>Message ID of the poll, or null if failed</returns>
    Task<ulong?> SendPollAsync(BotPoll poll);
    
    /// <summary>
    /// Stops an active poll and returns the results
    /// </summary>
    /// <param name="messageId">Poll message ID</param>
    /// <returns>Poll results with voter counts, or null if failed</returns>
    Task<DiscordPollResult?> StopPollAsync(ulong messageId);
    
    /// <summary>
    /// Sends a reply to a specific message
    /// </summary>
    /// <param name="message">Reply text</param>
    /// <param name="replyToMessageId">Message ID to reply to</param>
    /// <returns>Message ID of the reply, or null if failed</returns>
    Task<ulong?> SendReplyAsync(string message, ulong replyToMessageId);
}
