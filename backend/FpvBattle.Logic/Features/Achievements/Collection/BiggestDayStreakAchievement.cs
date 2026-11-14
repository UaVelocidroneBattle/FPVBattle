using FpvBattle.Domain;
using FpvBattle.Infrastructure.Persistence;
using FpvBattle.Logic.Features.Achievements.Base;
using FpvBattle.Logic.Features.Achievements.Services;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Logic.Features.Achievements.Collection;

public class BiggestDayStreakAchievement : IGlobalAchievement
{
    private readonly IApplicationDbContext _db;

    public BiggestDayStreakAchievement(
        IApplicationDbContext db)
    {
        _db = db;
    }

    public string Name => "BiggestDayStreak";
    public string Title => "President";
    public string Description => "Pilot with the biggest day streak";
    public string? CupId => null;

    public async Task<Pilot?> CheckAsync()
    {
        var topPilots = await _db.Pilots
            .Where(p => p.MaxDayStreak > 0)
            .OrderByDescending(p => p.MaxDayStreak)
            .Take(2)
            .ToListAsync();

        var leader = topPilots.FirstOrDefault();

        if (leader is null)
        {
            return null;
        }

        // A top streak shared by several pilots has no single owner,
        // so the title stays where it is until someone pulls strictly ahead.
        if (topPilots.Count > 1 && topPilots[1].MaxDayStreak == leader.MaxDayStreak)
        {
            return null;
        }

        var currentAchievement = await _db.PilotAchievements
            .FindByName(Name)
            .SingleOrDefaultAsync();

        if (currentAchievement is null)
        {
            leader.AddAchievement(this);
            return leader;
        }

        if (currentAchievement.Pilot.Name == leader.Name)
        {
            return null;
        }

        currentAchievement.Pilot = leader;
        currentAchievement.Date = DateTime.UtcNow;

        return leader;
    }
}
