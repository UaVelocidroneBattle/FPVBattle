using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Achievements.Base;

namespace Veloci.Data.Achievements;

public static class AchievementPackage
{
    public static IServiceCollection RegisterAchievements(this IServiceCollection services)
    {

        services
            .Add<DayStreak10Achievement>()
            .Add<DayStreak20Achievement>()
            .Add<DayStreak50Achievement>()
            .Add<DayStreak75Achievement>()
            .Add<DayStreak100Achievement>()
            .Add<DayStreak150Achievement>()
            .Add<DayStreak200Achievement>()
            .Add<DayStreak250Achievement>()
            .Add<DayStreak300Achievement>()
            .Add<DayStreak365Achievement>()
            .Add<DayStreak500Achievement>()
            .Add<DayStreak1000Achievement>()

            .Add<BiggestDayStreakAchievement>()
            ;

        return services;
    }

    private static IServiceCollection Add<T>(this IServiceCollection services) where T : IAchievement
    {
        services.AddScoped(typeof(IAchievement), typeof(T));
        return services;
    }
}
