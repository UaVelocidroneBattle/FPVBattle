namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Factory for creating cup-specific Discord bot instances
/// </summary>
/// <remarks>
/// Enables multi-cup support by routing messages to the appropriate Discord channel
/// based on cup configuration. Each cup can have its own dedicated Discord channel.
/// Lifecycle managed by DiscordBotHostedService.
/// </remarks>
public interface IDiscordBotFactory
{
    /// <summary>
    /// Initializes the Discord client connection
    /// </summary>
    /// <remarks>Called by DiscordBotHostedService at application startup</remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the Discord client connection
    /// </summary>
    /// <remarks>Called by DiscordBotHostedService at application shutdown</remarks>
    Task StopAsync();

    /// <summary>
    /// Gets a Discord bot instance configured for the specified cup
    /// </summary>
    /// <param name="cupId">Cup identifier (e.g., "5inch", "whoop")</param>
    /// <returns>Bot channel instance for the cup</returns>
    /// <exception cref="ArgumentException">Thrown when cup doesn't exist or has no Discord configuration</exception>
    IDiscordBot GetBotForCup(string cupId);

    /// <summary>
    /// Tries to get a bot instance for the specified cup
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="bot">Bot instance if successful</param>
    /// <returns>True if cup has Discord configuration, false otherwise</returns>
    bool TryGetBotForCup(string cupId, out IDiscordBot? bot);
}
