using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Logic.Services;

public class AchievementService
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IEnumerable<IAchievement> _achievements;

    public AchievementService(
        IRepository<Pilot> pilots,
        IServiceProvider serviceProvider)
    {
        _pilots = pilots;
        _achievements = serviceProvider.GetServices<IAchievement>();
    }

    public async Task CheckAsync()
    {
        var pilots = await _pilots.GetAll().ToListAsync();
        await CheckPilotsAchievements(pilots);
        await SelfCheckAsync();
        await _pilots.SaveChangesAsync();
    }

    private async Task CheckPilotsAchievements(List<Pilot> pilots)
    {
        var pilotCheckAchievements = _achievements.OfType<IAchievementPilotCheck>().ToArray();
        foreach (var pilot in pilots)
        {
            await CheckPilot(pilot, pilotCheckAchievements);
        }
    }

    private async Task CheckPilot(Pilot pilot, IAchievementPilotCheck[] pilotCheckAchievements)
    {
        foreach (var achievement in pilotCheckAchievements)
        {
            var triggered = await achievement.CheckAsync(pilot);

            if (!triggered)
                continue;

            var pilotAchievement = new PilotAchievement
            {
                Pilot = pilot,
                Date = DateTime.Now,
                Name = achievement.Name
            };

            pilot.Achievements.Add(pilotAchievement);
        }

        // broadcast an event and send a notification?
    }

    private async Task SelfCheckAsync()
    {
        var selfCheckAchievements = _achievements.OfType<IAchievementSelfCheck>();

        foreach (var achievement in selfCheckAchievements)
        {
            await achievement.CheckAsync();
        }
    }
}
