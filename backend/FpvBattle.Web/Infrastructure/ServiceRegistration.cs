using FpvBattle.Infrastructure.Imaging;
using FpvBattle.Infrastructure.Patreon;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Infrastructure.Velocidrone;
using FpvBattle.Application.API;
using FpvBattle.Application.Bot.Discord;
using FpvBattle.Application.Bot.Telegram;
using FpvBattle.Application.Features.Patreon;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Helpers;
using FpvBattle.Application.Services;
using FpvBattle.Application.Features.Achievements;
using FpvBattle.Application.Features.Leagues;
using FpvBattle.Application.Features.QuadOfTheDay;
using FpvBattle.Application.Services.Pilots;
using FpvBattle.Application.Services.Statistics;
using FpvBattle.Application.Services.Tracks;
using FpvBattle.Web.Controllers.Api.Heatmap;
using FpvBattle.Application.Persistence;

namespace FpvBattle.Web.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection RegisterCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddVelocidroneClient(configuration);
        services.AddScoped<CompetitionService>();
        services.AddScoped<CompetitionConductor>();
        services.AddScoped<RaceResultsConverter>();
        services.AddScoped<TelegramMessageComposer>();
        services.AddScoped<DiscordMessageComposer>();
        services.AddSingleton<TelegramChatMessages>();
        services.AddSingleton<DiscordChatMessages>();
        services.AddScoped<RaceResultDeltaAnalyzer>();
        services.AddScoped<TrackService>();
        services.AddScoped<PilotResultsCalculator>();
        services.AddScoped<PilotService>();
        services.AddScoped<IPilotProfileService, PilotProfileService>();
        services.AddScoped<PilotPlatformsService>();
        services.AddScoped<PointsCalculator>();
        services.AddScoped<TrackQueueService>();
        services.AddScoped<ModelsService>();
        services.AddScoped<ILeaderboardCalculator, LeaderboardCalculator>();
        services.AddScoped<ParticipationStatisticsService>();
        services.AddScoped<PilotsCountStatisticsService>();

        services.AddAchievementsServices(configuration);
        services.AddPatreonServices(configuration);
        services.AddPatreonApi();
        services.AddImaging();
        services.AddCups(configuration);
        services.AddLeagues(configuration);
        services.AddQuadOfTheDay();

        return services;
    }
}
