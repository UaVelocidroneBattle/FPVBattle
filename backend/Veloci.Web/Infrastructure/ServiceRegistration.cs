using Veloci.Data.Repositories;
using Veloci.Logic.Bot;
using Veloci.Logic.Bot.Telegram;
using Veloci.Logic.Helpers;
using Veloci.Logic.Services;
using Veloci.Logic.Services.Tracks;
using Veloci.Web.Controllers.Heatmap;

namespace Veloci.Web.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection services)
    {
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<ResultsFetcher>();
        services.AddTransient<CompetitionService>();
        services.AddTransient<CompetitionConductor>();
        services.AddTransient<RaceResultsConverter>();
        services.AddTransient<TelegramMessageComposer>();
        services.AddTransient<DiscordMessageComposer>();
        services.AddTransient<RaceResultDeltaAnalyzer>();
        services.AddTransient<TelegramBot>();
        services.AddTransient<ITelegramUpdateHandler, TelegramUpdateHandler>();
        services.AddTransient<ImageService>();
        //services.AddTransient<ITrackFetcher, WebTrackFetcher>();
        services.AddTransient<ITrackFetcher, ApiTrackFetcher>();
        services.AddTransient<TrackService>();
        services.AddTransient<PilotResultsCalculator>();

        return services;
    }
}
