using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Achievements.Collection;
using Veloci.Logic.Features.Achievements.Collection.OpenClass;
using Veloci.Logic.Features.Achievements.Collection.WhoopClass;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Features.Achievements.NotificationHandlers;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements;

public static class AchievementsServiceExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAchievementsServices(IConfiguration configuration)
        {
            // Register core services
            services.AddScoped<AchievementService>();
            services.AddScoped<AchievementsEventHandler>();
            services.AddScoped<IPilotCupLookupService, PilotCupLookupService>();

            // Register message composers
            services.AddScoped<DiscordAchievementMessageComposer>();
            services.AddScoped<TelegramAchievementMessageComposer>();

            // Register notification handlers
            services.AddScoped<DiscordAchievementsHandler>();
            services.AddScoped<TelegramAchievementsHandler>();

            var cupsConfig = configuration.GetSection(CupsConfiguration.SectionName).Get<CupsConfiguration>() ?? new();

            // Register all achievements
            services
                // day streaks
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
                ;

            // Open class achievements
            if (IsCupEnabled(CupIds.OpenClass))
            {
                services
                    .AddAchievement<ThirdPlaceInRace_Open_Achievement>()
                    .AddAchievement<SecondPlaceInRace_Open_Achievement>()
                    .AddAchievement<FirstPlaceInRace_Open_Achievement>()
                    .AddAchievement<ThirdInSeason_Open_Achievement>()
                    .AddAchievement<SecondInSeason_Open_Achievement>()
                    .AddAchievement<FirstInSeason_Open_Achievement>()
                    ;
            }

            // Whoop class achievements
            if (IsCupEnabled(CupIds.WhoopClass))
            {
                services
                    .AddAchievement<ThirdPlaceInRace_Whoop_Achievement>()
                    .AddAchievement<SecondPlaceInRace_Whoop_Achievement>()
                    .AddAchievement<FirstPlaceInRace_Whoop_Achievement>()
                    .AddAchievement<ThirdInSeason_Whoop_Achievement>()
                    .AddAchievement<SecondInSeason_Whoop_Achievement>()
                    .AddAchievement<FirstInSeason_Whoop_Achievement>()
                    ;
            }

            // Others
            services
                .AddAchievement<LastInRaceAchievement>()
                .AddAchievement<BiggestDayStreakAchievement>()
                .AddAchievement<GlobalFirstPlaceAchievement>()
                .AddAchievement<EarlyBirdAchievement>()
                .AddAchievement<LateBirdAchievement>()
                .AddAchievement<FirstResultAchievement>()
                .AddAchievement<MedalistAchievement>()
                //.AddAchievement<OvertakeTheDuckAchievement>()
                .AddAchievement<JackpotAchievement>()
                .AddAchievement<NanoBoostAchievement>()
                .AddAchievement<BeastAchievement>()
                .AddAchievement<UniversalSoldierAchievement>()
                ;

            return services;

            bool IsCupEnabled(string cupId) =>
                cupsConfig.Definitions.TryGetValue(cupId, out var cup) && cup.IsEnabled;
        }

        private IServiceCollection AddAchievement<T>() where T : IAchievement
        {
            services.AddScoped(typeof(IAchievement), typeof(T));
            return services;
        }
    }
}
