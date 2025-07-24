using Microsoft.Extensions.DependencyInjection;

namespace Veloci.Data.Achievements.Base;

public static class AchievementPackage
{
    public static IServiceCollection RegisterAchievements(this IServiceCollection services)
    {

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
