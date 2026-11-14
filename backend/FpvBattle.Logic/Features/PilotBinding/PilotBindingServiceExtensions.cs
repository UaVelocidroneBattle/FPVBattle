using FpvBattle.Logic.Features.PilotBinding.Jobs;
using FpvBattle.Logic.Features.PilotBinding.Services;
using FpvBattle.Logic.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Logic.Features.PilotBinding;

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
