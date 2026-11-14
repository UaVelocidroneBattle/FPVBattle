using FpvBattle.Application.Features.Leagues.Jobs;
using FpvBattle.Application.Features.Leagues.NotificationHandlers;
using FpvBattle.Application.Features.Leagues.Services;
using FpvBattle.Application.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FpvBattle.Application.Features.Leagues;

public static class LeaguesServiceExtensions
{
    public static IServiceCollection AddLeagues(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJobRegistrar, LeagueJobsRegistrar>();
        services.Configure<PaceRatingSettings>(configuration.GetSection(PaceRatingSettings.SectionName));
        services.AddScoped<PaceRatingCalculator>();
        services.AddScoped<RatingService>();
        services.AddScoped<RatingQualificationService>();
        services.AddScoped<LeagueService>();
        services.AddScoped<TelegramLeagueHandler>();
        services.AddScoped<DiscordLeagueHandler>();
        services.AddScoped<InAppLeagueHandler>();
        services.AddScoped<InAppLeagueMessageComposer>();

        return services;
    }
}
