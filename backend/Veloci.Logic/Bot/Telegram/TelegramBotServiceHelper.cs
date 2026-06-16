using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;

namespace Veloci.Logic.Bot.Telegram;

public static class TelegramBotServiceHelper
{
    public static IServiceCollection UseTelegramBotService(this IServiceCollection services)
    {
        // Register ITelegramBotClient as singleton (required - application will fail to start if not configured)
        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var botToken = config.GetSection("Telegram:BotToken").Value;

            if (string.IsNullOrEmpty(botToken))
            {
                Log.Error("Telegram:BotToken not configured. Running without Telegram support.");
                return null!;
            }

            Log.Information("Telegram bot client created with configured token");
            return new TelegramBotClient(botToken);
        });

        services.AddScoped<ITelegramMessenger, TelegramMessenger>();
        services.AddScoped<ITelegramCupMessenger, TelegramCupMessenger>();
        services.AddHostedService<TelegramBotHostedService>();
        services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();

        return services;
    }
}
