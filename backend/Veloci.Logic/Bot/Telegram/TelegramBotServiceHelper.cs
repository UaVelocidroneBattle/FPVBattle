using Microsoft.Extensions.DependencyInjection;

namespace Veloci.Logic.Bot.Telegram;

public static class TelegramBotServiceHelper
{
    public static IServiceCollection UseTelegramBotService(this IServiceCollection services)
    {
        services.AddSingleton<ITelegramBotFactory, TelegramBotFactory>();
        services.AddHostedService<TelegramBotHostedService>();

        return services;
    }
}
