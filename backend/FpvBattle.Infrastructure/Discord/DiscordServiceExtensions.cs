using FpvBattle.Application.Bot.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Infrastructure.Discord;

public static class DiscordServiceExtensions
{
    public static IServiceCollection AddDiscordBot(this IServiceCollection services)
    {
        services.AddSingleton<IDiscordBotFactory, DiscordBotFactory>();
        services.AddHostedService<DiscordBotHostedService>();

        return services;
    }
}
