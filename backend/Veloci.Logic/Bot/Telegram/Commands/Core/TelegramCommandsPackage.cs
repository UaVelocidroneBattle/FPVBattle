using Microsoft.Extensions.DependencyInjection;

namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public static class TelegramCommandsPackage
{
    public static IServiceCollection RegisterTelegramCommands(this IServiceCollection services)
    {
        services
            .AddScoped<TelegramCommandProcessor>()
            .AddScoped<ITelegramCommand, HelpCommand>()
            .AddScoped<ITelegramCommand, PilotCommand>()
            .AddScoped<ITelegramCommand, AchievementsCommand>()
            .AddScoped<ITelegramCommand, ConnectPilotToPlatformCommand>()
            .AddScoped<ITelegramCommand, ConnectPilotToPatreonCommand>()
            .AddScoped<ITelegramCommand, ListPatreonSupportersCommand>()
            .AddScoped<ITelegramCommand, ListPilotPlatformsCommand>()
            ;

        return services;
    }
}
