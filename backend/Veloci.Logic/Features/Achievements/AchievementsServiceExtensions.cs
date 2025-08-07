using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Achievements.Collection;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Features.Achievements.NotificationHandlers;
using Veloci.Logic.Features.Achievements.Jobs;
using Veloci.Logic.Jobs;

namespace Veloci.Logic.Features.Achievements;

public static class AchievementsServiceExtensions
{
    public static IServiceCollection AddAchievementsServices(this IServiceCollection services)
    {
        // Register core services
        services.AddScoped<AchievementService>();
        services.AddScoped<AchievementsEventHandler>();

        // Register message composers
        services.AddScoped<DiscordAchievementMessageComposer>();
        services.AddScoped<TelegramAchievementMessageComposer>();

        // Register notification handlers
        services.AddScoped<DiscordAchievementsHandler>();
        services.AddScoped<TelegramAchievementsHandler>();

        // Register jobs
        services.AddScoped<DayStreakMilestoneJob>();
        services.AddScoped<IJobRegistrar, AchievementsJobRegistrar>();

        // Register all achievements
        services
            .AddAchievement<DayStreak10Achievement>()
            .AddAchievement<DayStreak20Achievement>()
            .AddAchievement<DayStreak50Achievement>()
            .AddAchievement<DayStreak75Achievement>()
            .AddAchievement<DayStreak100Achievement>()
            .AddAchievement<DayStreak150Achievement>()
            .AddAchievement<DayStreak200Achievement>()
            .AddAchievement<DayStreak250Achievement>()
            .AddAchievement<DayStreak300Achievement>()
            .AddAchievement<DayStreak365Achievement>()
            .AddAchievement<DayStreak500Achievement>()
            .AddAchievement<DayStreak1000Achievement>()
            .AddAchievement<ThirdPlaceInRaceAchievement>()
            .AddAchievement<SecondPlaceInRaceAchievement>()
            .AddAchievement<FirstPlaceInRaceAchievement>()
            .AddAchievement<LastInRaceAchievement>()
            .AddAchievement<ThirdInSeasonAchievement>()
            .AddAchievement<SecondInSeasonAchievement>()
            .AddAchievement<FirstInSeasonAchievement>()
            .AddAchievement<BiggestDayStreakAchievement>()
            .AddAchievement<GlobalFirstPlaceAchievement>()
            .AddAchievement<EarlyBirdAchievement>()
            .AddAchievement<LateBirdAchievement>()
            .AddAchievement<FirstResultAchievement>()
            .AddAchievement<OvertakeTheDuckAchievement>()
            ;

        return services;
    }

    private static IServiceCollection AddAchievement<T>(this IServiceCollection services) where T : IAchievement
    {
        services.AddScoped(typeof(IAchievement), typeof(T));
        return services;
    }
}
