using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Veloci.Logic.Bot.Telegram;

public class TelegramBotHostedService : IHostedService, IDisposable
{
    private static readonly ILogger _log = Log.ForContext<TelegramBotHostedService>();
    
    private TelegramBot _telegramBot;
    private IServiceScope _scope;

    public TelegramBotHostedService(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        _telegramBot = _scope.ServiceProvider.GetRequiredService<TelegramBot>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Bot service TelegramBotHostedService starting...");
            _telegramBot.Init();
            _log.Information("✅ Bot service TelegramBotHostedService started successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Bot service TelegramBotHostedService encountered issues during startup");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Bot service TelegramBotHostedService stopping...");
            _telegramBot.Stop();
            _log.Information("🛑 Bot service TelegramBotHostedService stopped gracefully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Bot service TelegramBotHostedService encountered issues during shutdown");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _log.Debug("Disposing TelegramBotHostedService resources");
            _scope.Dispose();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error disposing TelegramBotHostedService resources");
        }
    }
}
