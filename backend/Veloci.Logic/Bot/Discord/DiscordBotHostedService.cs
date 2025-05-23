using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Veloci.Logic.Bot.Discord;

namespace Veloci.Logic.Bot;

public class DiscordBotHostedService : IHostedService, IDisposable
{
    private readonly DiscordBot _bot;
    private readonly IServiceScope _scope;

    public DiscordBotHostedService(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        _bot = (DiscordBot)_scope.ServiceProvider.GetRequiredService<IDiscordBot>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bot.StartAsync();
        Log.Information("Starting discord bot");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Stopping discord bot");
        await _bot.Stop();
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
