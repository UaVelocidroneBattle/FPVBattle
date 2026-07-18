using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.PilotBinding.Jobs;
using Veloci.Logic.Features.PilotBinding.Services;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.PilotBinding;

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
