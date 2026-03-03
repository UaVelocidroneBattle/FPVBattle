using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Cups.Jobs;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Cups;

public static class CupsServiceExtensions
{
    /// <summary>
    /// Registers cup-related services and configuration
    /// </summary>
    public static IServiceCollection AddCups(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<CupsConfiguration>(configuration.GetSection(CupsConfiguration.SectionName));

        // Register services
        services.AddSingleton<ICupService, CupService>();
        services.AddSingleton<ICupContextResolver, CupContextResolver>();

        // Register job registrar for dynamic job scheduling
        services.AddTransient<IJobRegistrar, CupJobRegistrar>();

        return services;
    }
}
