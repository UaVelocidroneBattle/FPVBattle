using FpvBattle.Application.Features.PilotBinding.Jobs;
using FpvBattle.Application.Features.PilotBinding.Services;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.PilotBinding;

public static class PilotBindingServiceExtensions
{
    public static IServiceCollection AddPilotBinding(this IServiceCollection services)
    {
        services.AddScoped<PilotBindingService>();
        services.AddScoped<ExpiredClaimCleanupJob>();
        services.AddScoped<IJobRegistrar, PilotBindingJobRegistrar>();
        return services;
    }
}
