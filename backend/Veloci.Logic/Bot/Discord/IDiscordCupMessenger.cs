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
}
