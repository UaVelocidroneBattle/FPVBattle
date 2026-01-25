using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Serilog;
using Telegram.Bot;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Factory for creating cup-specific Telegram bot channel instances
/// </summary>
public class TelegramBotFactory : ITelegramBotFactory
{
    private static readonly ILogger _log = Log.ForContext<TelegramBotFactory>();

    private readonly ICupService _cupService;
    private readonly TelegramBotClient _client;
    private readonly ConcurrentDictionary<string, ITelegramBotChannel> _channelCache;

    public TelegramBotFactory(ICupService cupService, IConfiguration configuration)
    {
        _cupService = cupService ?? throw new ArgumentNullException(nameof(cupService));

        var botToken = configuration.GetSection("Telegram:BotToken").Value;
        if (string.IsNullOrEmpty(botToken))
        {
            throw new InvalidOperationException("Telegram bot token is not configured");
        }

        _client = new TelegramBotClient(botToken);
        _channelCache = new ConcurrentDictionary<string, ITelegramBotChannel>();

        _log.Information("TelegramBotFactory initialized with bot token");
    }

    public ITelegramBotChannel GetBotForCup(string cupId)
    {
        if (string.IsNullOrEmpty(cupId))
        {
            throw new ArgumentException("Cup ID cannot be null or empty", nameof(cupId));
        }

        // Thread-safe get-or-add pattern
        return _channelCache.GetOrAdd(cupId, id =>
        {
            // Get cup configuration
            var cupOptions = _cupService.GetCupOptions(id);

            if (cupOptions.Channels?.Telegram?.ChannelId == null)
            {
                throw new ArgumentException($"Cup '{id}' does not have Telegram channel configured", nameof(cupId));
            }

            if (string.IsNullOrEmpty(cupOptions.Channels.Telegram.ChannelId))
            {
                throw new ArgumentException($"Cup '{id}' has empty Telegram channel ID", nameof(cupId));
            }

            // Create new bot channel instance
            var botChannel = new TelegramBotChannel(_client, cupOptions.Channels.Telegram.ChannelId);

            _log.Information("Created Telegram bot channel for cup {CupId} with channel ID {ChannelId}",
                id, cupOptions.Channels.Telegram.ChannelId);

            return botChannel;
        });
    }

    public bool TryGetBotForCup(string cupId, out ITelegramBotChannel? bot)
    {
        bot = null;

        if (string.IsNullOrEmpty(cupId))
        {
            return false;
        }

        try
        {
            // Check cache first (fast path)
            if (_channelCache.TryGetValue(cupId, out bot))
            {
                return true;
            }

            // Check if cup exists and has Telegram configuration
            if (!_cupService.CupExists(cupId))
            {
                _log.Warning("Cup {CupId} does not exist", cupId);
                return false;
            }

            var cupOptions = _cupService.GetCupOptions(cupId);

            if (cupOptions.Channels?.Telegram?.ChannelId == null ||
                string.IsNullOrEmpty(cupOptions.Channels.Telegram.ChannelId))
            {
                _log.Warning("Cup {CupId} does not have Telegram channel configured", cupId);
                return false;
            }

            // Thread-safe get-or-add (handles race conditions)
            bot = _channelCache.GetOrAdd(cupId, id =>
            {
                var channel = new TelegramBotChannel(_client, cupOptions.Channels.Telegram.ChannelId);
                _log.Information("Created Telegram bot channel for cup {CupId} with channel ID {ChannelId}",
                    id, cupOptions.Channels.Telegram.ChannelId);
                return channel;
            });

            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get Telegram bot for cup {CupId}", cupId);
            return false;
        }
    }
}
