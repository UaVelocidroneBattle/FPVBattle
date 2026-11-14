using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.QuadOfTheDay;

public static class QuadOfTheDayExtensions
{
    public static IServiceCollection AddQuadOfTheDay(this IServiceCollection services)
    {
        services.AddScoped<QuadOfTheDayService>();

        return services;
    }
}
