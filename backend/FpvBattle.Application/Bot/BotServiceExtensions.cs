using FpvBattle.Application.Bot.Discord;
using FpvBattle.Application.Bot.Telegram;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Bot;

public static class BotServiceExtensions
{
    public static IServiceCollection AddBotMessaging(this IServiceCollection services)
    {
        services.AddScoped<ITelegramCupMessenger, TelegramCupMessenger>();
        services.AddScoped<IDiscordCupMessenger, DiscordCupMessenger>();
        services.AddScoped<IDiscordGeneralMessenger, DiscordGeneralMessenger>();

        return services;
    }
}
