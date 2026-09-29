using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Achievements.Collection;
using Veloci.Logic.Features.Achievements.Collection.Leagues;
using Veloci.Logic.Features.Achievements.Collection.WhoopClass;
using Veloci.Logic.Features.Achievements.Jobs;
using Veloci.Logic.Features.Achievements.Services;
using Veloci.Logic.Features.Achievements.NotificationHandlers;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Jobs;

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

            // One-off cleanup of obsolete achievements; delete once it has run in production
            services.AddScoped<AchievementCleanupJob>();
            services.AddScoped<IJobRegistrar, AchievementsJobRegistrar>();

            // Register message composers
            services.AddScoped<DiscordAchievementMessageComposer>();
            services.AddScoped<TelegramAchievementMessageComposer>();
            services.AddScoped<InAppAchievementMessageComposer>();

            // Register notification handlers
            services.AddScoped<DiscordAchievementsHandler>();
            services.AddScoped<TelegramAchievementsHandler>();
            services.AddScoped<InAppAchievementsHandler>();

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
                var leagues = cupsConfig.Definitions[CupIds.OpenClass].Leagues;

                foreach (var league in leagues.Definitions)
                {
                    for (var place = 1; place <= 3; place++)
                    {
                        services.AddAchievement(new LeagueRacePlacementAchievement(CupIds.OpenClass, league.Name, league.Name, place));
                        services.AddAchievement(new LeagueSeasonPlacementAchievement(CupIds.OpenClass, league.Name, place));
                    }
                }

                // Unranked pilots only get a winner achievement
                services.AddAchievement(new LeagueRacePlacementAchievement(CupIds.OpenClass, null, leagues.OthersName, 1));
                services.AddAchievement(new LeagueSeasonPlacementAchievement(CupIds.OpenClass, leagues.OthersName, 1));
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
                    .AddAchievement<FirstInSeason_Whoop_Achievement>();
            }

            // Others
            services
                .AddAchievement<LastInRaceAchievement>()
                .AddAchievement<BiggestDayStreakAchievement>()
                .AddAchievement<GlobalFirstPlaceAchievement>()
                .AddAchievement<EarlyBirdAchievement>()
                .AddAchievement<LateBirdAchievement>()
                .AddAchievement<FirstResultAchievement>()
                .AddAchievement<JackpotAchievement>()
                .AddAchievement<NanoBoostAchievement>()
                .AddAchievement<BeastAchievement>()
                .AddAchievement<UniversalSoldierAchievement>()
                .AddAchievement<LevelUpAchievement>()
                .AddAchievement<DemotedAchievement>()
                .AddAchievement<RankedAchievement>()
                .AddAchievement<DroppedAchievement>()
                ;

            return services;

            bool IsCupEnabled(string cupId) =>
                cupsConfig.Definitions.TryGetValue(cupId, out var cup) && cup.IsEnabled;
        }

        private IServiceCollection AddAchievement(IAchievement instance)
        {
            services.AddScoped<IAchievement>(_ => instance);
            return services;
        }

        private IServiceCollection AddAchievement<T>() where T : IAchievement
        {
            services.AddScoped(typeof(IAchievement), typeof(T));
            return services;
        }
    }
}
