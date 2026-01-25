using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Factory for creating cup-specific Discord bot channel instances
/// </summary>
/// <remarks>
/// Shares a single Discord socket client across all channels for efficiency,
/// but creates channel-specific wrappers for routing messages to the correct Discord channel.
/// </remarks>
public class DiscordBotFactory : IDiscordBotFactory
{
    private static readonly ILogger _log = Log.ForContext<DiscordBotFactory>();

    private readonly ICupService _cupService;
    private readonly DiscordSocketClient? _client;
    private readonly string? _botToken;
    private readonly ConcurrentDictionary<string, IDiscordBot> _channelCache;
    private bool _isInitialized;
    private TaskCompletionSource? _readyCompletionSource;

    public DiscordBotFactory(ICupService cupService, IConfiguration configuration)
    {
        _cupService = cupService ?? throw new ArgumentNullException(nameof(cupService));
        _channelCache = new ConcurrentDictionary<string, IDiscordBot>();

        _botToken = configuration.GetSection("Discord:BotToken").Value;

        if (string.IsNullOrEmpty(_botToken))
        {
            _log.Information("Discord bot token not configured, Discord support will be disabled");
            _client = null;
            return;
        }

        // Only create and configure the client, don't connect yet
        _client = new DiscordSocketClient();
        _client.Log += LogDiscordMessage;
        _client.Ready += OnBotReady;

        _log.Information("DiscordBotFactory created (initialization will happen via HostedService)");
    }

    /// <summary>
    /// Initializes the Discord client connection. Called by DiscordBotHostedService.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            _log.Debug("Discord client not configured, skipping initialization");
            return;
        }

        if (_isInitialized)
        {
            _log.Warning("Discord client already initialized, skipping");
            return;
        }

        try
        {
            _log.Information("Initializing Discord client connection...");

            // Create ready completion source
            _readyCompletionSource = new TaskCompletionSource();

            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();

            // Wait for Ready event with timeout
            var readyTask = _readyCompletionSource.Task;
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

            var completedTask = await Task.WhenAny(readyTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("Discord client failed to become ready within 30 seconds");
            }

            _isInitialized = true;
            _log.Information("✅ Discord client initialized and ready");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "❌ Failed to initialize Discord client");
            throw;
        }
    }

    private Task OnBotReady()
    {
        _log.Information("Discord bot Ready event fired");
        _readyCompletionSource?.TrySetResult();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_client is not null && _isInitialized)
        {
            _log.Information("Stopping Discord client...");
            await _client.StopAsync();
            _isInitialized = false;
            _log.Information("Discord client stopped");
        }
    }

    private static Task LogDiscordMessage(LogMessage msg)
    {
        _log.Verbose("Discord: {Message}", msg.ToString());
        return Task.CompletedTask;
    }

    public IDiscordBot GetBotForCup(string cupId)
    {
        if (_client is null)
        {
            throw new InvalidOperationException("Discord bot is not configured");
        }

        if (string.IsNullOrEmpty(cupId))
        {
            throw new ArgumentException("Cup ID cannot be null or empty", nameof(cupId));
        }

        // Thread-safe get-or-add pattern
        return _channelCache.GetOrAdd(cupId, id =>
        {
            // Get cup configuration
            var cupOptions = _cupService.GetCupOptions(id);

            if (cupOptions.Channels?.Discord?.Channel == null)
            {
                throw new ArgumentException($"Cup '{id}' does not have Discord channel configured", nameof(cupId));
            }

            if (string.IsNullOrEmpty(cupOptions.Channels.Discord.Channel))
            {
                throw new ArgumentException($"Cup '{id}' has empty Discord channel name", nameof(cupId));
            }

            // Create new bot channel instance
            var botChannel = new DiscordBotChannel(_client, cupOptions.Channels.Discord.Channel);

            _log.Information("Created Discord bot channel for cup {CupId} with channel name {ChannelName}",
                id, cupOptions.Channels.Discord.Channel);

            return botChannel;
        });
    }

    public bool TryGetBotForCup(string cupId, out IDiscordBot? bot)
    {
        bot = null;

        if (_client is null)
        {
            _log.Debug("Discord client is not configured");
            return false;
        }

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

            // Check if cup exists and has Discord configuration
            if (!_cupService.CupExists(cupId))
            {
                _log.Warning("Cup {CupId} does not exist", cupId);
                return false;
            }

            var cupOptions = _cupService.GetCupOptions(cupId);

            if (cupOptions.Channels?.Discord?.Channel == null ||
                string.IsNullOrEmpty(cupOptions.Channels.Discord.Channel))
            {
                _log.Warning("Cup {CupId} does not have Discord channel configured", cupId);
                return false;
            }

            // Thread-safe get-or-add (handles race conditions)
            bot = _channelCache.GetOrAdd(cupId, id =>
            {
                var channel = new DiscordBotChannel(_client, cupOptions.Channels.Discord.Channel);
                _log.Information("Created Discord bot channel for cup {CupId} with channel name {ChannelName}",
                    id, cupOptions.Channels.Discord.Channel);
                return channel;
            });

            return true;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get Discord bot for cup {CupId}", cupId);
            return false;
        }
    }
}
