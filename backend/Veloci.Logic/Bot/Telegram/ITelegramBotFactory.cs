namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Factory for creating cup-specific Telegram bot instances
/// </summary>
/// <remarks>
/// Enables multi-cup support by routing messages to the appropriate Telegram channel
/// based on cup configuration. Each cup can have its own dedicated Telegram channel.
/// </remarks>
public interface ITelegramBotFactory
{
    /// <summary>
    /// Gets a Telegram bot instance configured for the specified cup
    /// </summary>
    /// <param name="cupId">Cup identifier (e.g., "5inch", "whoop")</param>
    /// <returns>Bot channel instance for the cup</returns>
    /// <exception cref="ArgumentException">Thrown when cup doesn't exist or has no Telegram configuration</exception>
    ITelegramBotChannel GetBotForCup(string cupId);

    /// <summary>
    /// Tries to get a bot instance for the specified cup
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="bot">Bot instance if successful</param>
    /// <returns>True if cup has Telegram configuration, false otherwise</returns>
    bool TryGetBotForCup(string cupId, out ITelegramBotChannel? bot);
}
