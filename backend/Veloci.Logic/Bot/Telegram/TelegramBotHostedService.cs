using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Manages Telegram bot lifecycle - starts receiving updates on startup, stops on shutdown
/// </summary>
public class TelegramBotHostedService : IHostedService
{
    private static readonly ILogger _log = Log.ForContext<TelegramBotHostedService>();

    private readonly ITelegramBotClient? _client;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cts;

    public TelegramBotHostedService(ITelegramBotClient? client, IServiceProvider serviceProvider)
    {
        _client = client;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            _log.Information("Telegram not configured, skipping bot initialization");
            return Task.CompletedTask;
        }

        try
        {
            _log.Information("Initializing Telegram bot...");

            _cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            _client.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                _cts.Token);

            _log.Information("✅ Telegram bot initialized and receiving updates");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "❌ Telegram bot initialization failed");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            return Task.CompletedTask;
        }

        try
        {
            _log.Information("Stopping Telegram bot...");

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _log.Information("🛑 Telegram bot stopped gracefully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error during Telegram bot shutdown");
        }

        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ITelegramUpdateHandler>();
        await handler.OnUpdateAsync(botClient, update, cancellationToken);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _log.Error(exception, "Error in Telegram bot receiver");
        return Task.CompletedTask;
    }
}
