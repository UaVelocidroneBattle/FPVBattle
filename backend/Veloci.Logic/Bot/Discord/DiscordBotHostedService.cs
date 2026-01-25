using Microsoft.Extensions.Hosting;
using Serilog;
using Veloci.Logic.Bot.Discord;

namespace Veloci.Logic.Bot;

/// <summary>
/// Manages Discord bot factory lifecycle - initializes connection on startup, disconnects on shutdown
/// </summary>
public class DiscordBotHostedService : IHostedService
{
    private static readonly ILogger _log = Log.ForContext<DiscordBotHostedService>();

    private readonly IDiscordBotFactory _factory;

    public DiscordBotHostedService(IDiscordBotFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Initializing Discord bot factory...");
            await _factory.InitializeAsync(cancellationToken);
            _log.Information("✅ Discord bot factory initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "❌ Discord bot factory initialization failed");
            // Rethrow to prevent app from starting with broken Discord
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _log.Information("Stopping Discord bot factory...");
            await _factory.StopAsync();
            _log.Information("🛑 Discord bot factory stopped gracefully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error during Discord bot factory shutdown");
        }
    }
}
