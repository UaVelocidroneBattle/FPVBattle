using FpvBattle.Application.Features.Patreon.Jobs;
using FpvBattle.Application.Features.Patreon.Models;
using FpvBattle.Application.Features.Patreon.Services;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Patreon;

public static class PatreonServiceExtensions
{
    public static IServiceCollection AddPatreonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Patreon options
        services.Configure<PatreonOptions>(configuration.GetSection(PatreonOptions.SectionName));

        // Register core services
        services.AddScoped<IPatreonService, PatreonService>();
        services.AddScoped<PatreonUserMatcher>();

        // Register MediatR command handler
        services.AddScoped<PatreonSyncJob>();

        // Register job registrar
        services.AddScoped<IJobRegistrar, PatreonJobRegistrar>();

        return services;
    }
}
