using Microsoft.EntityFrameworkCore;
using Veloci.Data.Achievements.Base;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;

namespace Veloci.Data.Achievements;

public class BiggestDayStreakAchievement : IAchievementSelfCheck
{
    private readonly IRepository<Pilot> _pilots;
    private readonly IRepository<PilotAchievement> _pilotAchievements;

    public BiggestDayStreakAchievement(
        IRepository<Pilot> pilots,
        IRepository<PilotAchievement> pilotAchievements)
    {
        _pilots = pilots;
        _pilotAchievements = pilotAchievements;
    }

    public string Name => "BiggestDayStreak";
    public string Title => "Biggest day streak";

    public async Task CheckAsync()
    {
        var pilotWithBiggestDayStreak = await _pilots
            .GetAll()
            .OrderByDescending(p => p.MaxDayStreak)
            .FirstOrDefaultAsync();

        if (pilotWithBiggestDayStreak is null)
            return;

        var currentAchievement = await _pilotAchievements
            .GetAll()
            .SingleOrDefaultAsync(pa => pa.Name == Name);

        if (currentAchievement is null)
        {
            pilotWithBiggestDayStreak.AddAchievement(this);
            return;
        }

        if (currentAchievement.Pilot.Name == pilotWithBiggestDayStreak.Name)
            return;

        currentAchievement.Pilot = pilotWithBiggestDayStreak;
        currentAchievement.Date = DateTime.Now;
    }
}
