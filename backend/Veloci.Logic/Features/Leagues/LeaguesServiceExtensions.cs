using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Leagues.Jobs;
using Veloci.Logic.Features.Leagues.NotificationHandlers;
using Veloci.Logic.Features.Leagues.Services;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Leagues;

public static class LeaguesServiceExtensions
{
    public static IServiceCollection AddLeagues(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJobRegistrar, LeagueJobsRegistrar>();
        services.Configure<PaceRatingSettings>(configuration.GetSection(PaceRatingSettings.SectionName));
        services.AddScoped<PaceRatingCalculator>();
        services.AddScoped<RatingService>();
        services.AddScoped<LeagueService>();
        services.AddScoped<TelegramLeagueHandler>();
        services.AddScoped<DiscordLeagueHandler>();

        return services;
    }
}
