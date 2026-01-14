using Veloci.Data.Repositories;
using Veloci.Logic.API;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Features.Patreon;
using Veloci.Logic.Helpers;
using Veloci.Logic.Services;
using Veloci.Logic.Features.Achievements;
using Veloci.Logic.Services.Leagues;
using Veloci.Logic.Services.Pilots;
using Veloci.Logic.Services.Tracks;
using Veloci.Web.Controllers.Heatmap;

namespace Veloci.Web.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<Velocidrone>();
        services.AddScoped<CompetitionService>();
        services.AddScoped<CompetitionConductor>();
        services.AddScoped<RaceResultsConverter>();
        services.AddScoped<TelegramMessageComposer>();
        services.AddScoped<DiscordMessageComposer>();
        services.AddScoped<RaceResultDeltaAnalyzer>();
        services.AddScoped<TelegramBot>();
        services.AddScoped<ITelegramUpdateHandler, TelegramUpdateHandler>();
        services.AddScoped<ImageService>();
        //services.AddScoped<ITrackFetcher, WebTrackFetcher>();
        services.AddScoped<ITrackFetcher, ApiTrackFetcher>();
        services.AddScoped<TrackService>();
        services.AddScoped<PilotResultsCalculator>();
        services.AddScoped<PilotService>();
        services.AddScoped<IPilotProfileService, PilotProfileService>();
        services.AddScoped<PilotPlatformsService>();
        services.AddScoped<PointsCalculator>();
        services.AddScoped<LeagueService>();
        services.AddScoped<LeagueQualifier>();
        services.AddAchievementsServices();
        services.AddPatreonServices(configuration);

        return services;
    }
}
