using Microsoft.Extensions.DependencyInjection;

namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public static class TelegramCommandsPackage
{
    public static IServiceCollection RegisterTelegramCommands(this IServiceCollection services)
    {
        services
            .AddScoped<TelegramCommandProcessor>()
            .AddScoped<ITelegramCommand, HelpCommand>()
            .AddScoped<ITelegramCommand, TotalFlightDaysCommand>()
            .AddScoped<ITelegramCommand, DayStreakCommand>()
            .AddScoped<ITelegramCommand, AchievementsCommand>()
            ;

        return services;
    }
}
