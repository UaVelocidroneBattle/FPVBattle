using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Leagues.Services;

namespace Veloci.Logic.Features.Leagues;

public static class LeaguesServiceExtensions
{
    public static IServiceCollection AddLeagues(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaceRatingSettings>(configuration.GetSection(PaceRatingSettings.SectionName));
        services.AddScoped<PaceRatingCalculator>();
        services.AddScoped<RatingService>();

        return services;
    }
}
