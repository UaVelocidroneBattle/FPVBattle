using System.Diagnostics.CodeAnalysis;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Factory for creating Discord bot instances for cup-specific and general channels
/// </summary>
/// <remarks>
/// Enables multi-cup support by routing messages to the appropriate Discord channel
/// based on cup configuration. Each cup can have its own dedicated Discord channel.
/// A separate general channel is available for announcements not tied to a specific cup.
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
    /// <param name="cupId">Cup identifier (e.g., "open-class", "whoop")</param>
    /// <returns>Bot channel instance for the cup</returns>
    /// <exception cref="ArgumentException">Thrown when cup doesn't exist or has no Discord configuration</exception>
    IDiscordBot GetBotForCup(string cupId);

    /// <summary>
    /// Tries to get a bot instance for the specified cup
    /// </summary>
    /// <param name="cupId">Cup identifier</param>
    /// <param name="bot">Bot instance if successful</param>
    /// <returns>True if cup has Discord configuration, false otherwise</returns>
    bool TryGetBotForCup(string cupId, [NotNullWhen(true)] out IDiscordBot? bot);

    /// <summary>
    /// Tries to get a bot instance for the general channel
    /// </summary>
    /// <param name="bot">Bot instance if successful</param>
    /// <returns>True if Discord:GeneralChannel is configured, false otherwise</returns>
    bool TryGetGeneralBot([NotNullWhen(true)] out IDiscordBot? bot);
}
