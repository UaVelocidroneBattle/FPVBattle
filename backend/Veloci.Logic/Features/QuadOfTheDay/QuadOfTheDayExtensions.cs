using Microsoft.Extensions.DependencyInjection;

namespace Veloci.Logic.Features.QuadOfTheDay;

public static class QuadOfTheDayExtensions
{
    public static IServiceCollection AddQuadOfTheDay(this IServiceCollection services)
    {
        services.AddScoped<QuadOfTheDayService>();

        return services;
    }
}
