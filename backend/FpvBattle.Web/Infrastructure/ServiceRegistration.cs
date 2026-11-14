using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.API;
using FpvBattle.Logic.API.Velocidrone;
using FpvBattle.Logic.Bot.Discord;
using FpvBattle.Logic.Bot.Telegram;
using FpvBattle.Logic.Features.Patreon;
using FpvBattle.Logic.Features.Cups;
using FpvBattle.Logic.Helpers;
using FpvBattle.Logic.Services;
using FpvBattle.Logic.Features.Achievements;
using FpvBattle.Logic.Features.Leagues;
using FpvBattle.Logic.Features.QuadOfTheDay;
using FpvBattle.Logic.Services.Pilots;
using FpvBattle.Logic.Services.Statistics;
using FpvBattle.Logic.Services.Tracks;
using FpvBattle.Web.Controllers.Api.Heatmap;
using FpvBattle.Logic.Persistence;

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
        services.AddScoped<ImageService>();
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
        services.AddCups(configuration);
        services.AddLeagues(configuration);
        services.AddQuadOfTheDay();

        return services;
    }
}
