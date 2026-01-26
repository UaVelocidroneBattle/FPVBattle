using Microsoft.Extensions.Options;

namespace Veloci.Logic.Features.Cups;

/// <summary>
/// Resolves cup context from chat IDs and channel names
/// </summary>
/// <remarks>
/// When the bot receives messages from Telegram or Discord, it needs to determine
/// which cup the message is associated with. This resolver provides reverse lookup
/// from chat/channel identifiers to cup IDs.
/// </remarks>
public interface ICupContextResolver
{
    /// <summary>
    /// Resolves cup ID from Telegram chat ID
    /// </summary>
    /// <param name="chatId">Telegram chat ID (e.g., "-1001234567890")</param>
    /// <returns>Cup ID if found, null if chat is not bound to any cup</returns>
    string? GetCupIdByChatId(string chatId);

    /// <summary>
    /// Resolves cup ID from Discord channel name
    /// </summary>
    /// <param name="channelName">Discord channel name (e.g., "5inch-daily")</param>
    /// <returns>Cup ID if found, null if channel is not bound to any cup</returns>
    string? GetCupIdByDiscordChannel(string channelName);
}

public class CupContextResolver : ICupContextResolver
{
    private readonly Dictionary<string, string> _telegramChatToCup;
    private readonly Dictionary<string, string> _discordChannelToCup;

    public CupContextResolver(IOptions<CupsConfiguration> config)
    {
        var cupsConfig = config.Value;

        // Build reverse lookup maps for fast resolution
        _telegramChatToCup = cupsConfig.Definitions
            .Where(kvp => kvp.Value.Channels?.Telegram?.ChannelId != null)
            .ToDictionary(
                kvp => kvp.Value.Channels.Telegram!.ChannelId,
                kvp => kvp.Key,
                StringComparer.OrdinalIgnoreCase
            );

        _discordChannelToCup = cupsConfig.Definitions
            .Where(kvp => kvp.Value.Channels?.Discord?.Channel != null)
            .ToDictionary(
                kvp => kvp.Value.Channels.Discord!.Channel,
                kvp => kvp.Key,
                StringComparer.OrdinalIgnoreCase
            );
    }

    public string? GetCupIdByChatId(string chatId)
    {
        return _telegramChatToCup.GetValueOrDefault(chatId);
    }

    public string? GetCupIdByDiscordChannel(string channelName)
    {
        return _discordChannelToCup.GetValueOrDefault(channelName);
    }
}
