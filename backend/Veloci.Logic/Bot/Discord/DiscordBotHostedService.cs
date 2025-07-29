using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Veloci.Logic.Bot.Discord;

namespace Veloci.Logic.Bot;

public class DiscordBotHostedService : IHostedService, IDisposable
{
    private static readonly ILogger _log = Log.ForContext<DiscordBotHostedService>();
    
    private readonly DiscordBot _bot;
    private readonly IServiceScope _scope;

    public DiscordBotHostedService(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        _bot = (DiscordBot)_scope.ServiceProvider.GetRequiredService<IDiscordBot>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Bot service DiscordBotHostedService starting...");
            await _bot.StartAsync();
            _log.Information("✅ Bot service DiscordBotHostedService started successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Bot service DiscordBotHostedService encountered issues during startup");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Bot service DiscordBotHostedService stopping...");
            await _bot.Stop();
            _log.Information("🛑 Bot service DiscordBotHostedService stopped gracefully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Bot service DiscordBotHostedService encountered issues during shutdown");
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _log.Debug("Disposing DiscordBotHostedService resources");
            _scope.Dispose();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error disposing DiscordBotHostedService resources");
        }
    }
}
