using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Bot.Telegram.Commands.Core;

public static class TelegramCommandsPackage
{
    public static IServiceCollection RegisterTelegramCommands(this IServiceCollection services)
    {
        services
            .AddScoped<ITelegramCommand, HelpCommand>()
            .AddScoped<ITelegramCommand, PilotCommand>()
            .AddScoped<ITelegramCommand, AchievementsCommand>()
            .AddScoped<ITelegramCommand, CurrentTrackCommand>()
            .AddScoped<ITelegramCommand, ConnectPilotToPlatformCommand>()
            .AddScoped<ITelegramCommand, ListPilotPlatformsCommand>()
            ;

        return services;
    }
}
