namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Sends Discord messages to the general channel, not tied to any specific cup
/// </summary>
public interface IDiscordGeneralMessenger
{
    /// <summary>
    /// Sends a message to the general Discord channel.
    /// Does nothing if Discord:GeneralChannel is not configured.
    /// </summary>
    Task SendMessageAsync(string message);
}
