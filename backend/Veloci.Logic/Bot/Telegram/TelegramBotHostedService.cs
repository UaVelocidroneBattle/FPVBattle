using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Veloci.Logic.Bot.Telegram;

public class TelegramBotHostedService : IHostedService, IDisposable
{
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
            Log.Information("Bot service TelegramBotHostedService starting...");
            _telegramBot.Init();
            Log.Information("✅ Bot service TelegramBotHostedService started successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Bot service TelegramBotHostedService encountered issues during startup");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Bot service TelegramBotHostedService stopping...");
            _telegramBot.Stop();
            Log.Information("🛑 Bot service TelegramBotHostedService stopped gracefully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Bot service TelegramBotHostedService encountered issues during shutdown");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            Log.Debug("Disposing TelegramBotHostedService resources");
            _scope.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disposing TelegramBotHostedService resources");
        }
    }
}
