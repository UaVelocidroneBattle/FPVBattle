using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Achievements.Services;

namespace Veloci.Logic.Features.Achievements.Collection;

public class BiggestDayStreakAchievement : IGlobalAchievement
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
    public string Title => "Президент";
    public string Description => "Пілот з найбільшим дейстріком";

    public async Task CheckAsync()
    {
        var pilotWithBiggestDayStreak = await _pilots
            .GetAll()
            .OrderByDescending(p => p.MaxDayStreak)
            .FirstOrDefaultAsync();

        if (pilotWithBiggestDayStreak is null)
        {
            return;
        }

        var currentAchievement = await _pilotAchievements
            .GetAll()
            .FindByName(Name)
            .SingleOrDefaultAsync();

        if (currentAchievement is null)
        {
            pilotWithBiggestDayStreak.AddAchievement(this);
            return;
        }

        if (currentAchievement.Pilot.Name == pilotWithBiggestDayStreak.Name)
        {
            return;
        }

        currentAchievement.Pilot = pilotWithBiggestDayStreak;
        currentAchievement.Date = DateTime.Now;
    }
}
