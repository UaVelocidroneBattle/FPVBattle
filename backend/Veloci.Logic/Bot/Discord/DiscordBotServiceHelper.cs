using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Bot.Discord;

namespace Veloci.Logic.Bot;

public static class DiscordBotServiceHelper
{
    public static void UseDiscordBotService(this IServiceCollection services)
    {
        services.AddSingleton<IDiscordBotFactory, DiscordBotFactory>();

        // Register hosted services for bot lifecycle management
        services.AddHostedService<Bot.DiscordBotHostedService>();
    }
}
