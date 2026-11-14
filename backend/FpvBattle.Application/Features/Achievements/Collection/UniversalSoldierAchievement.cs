using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Application.Features.Cups;
using FpvBattle.Application.Persistence;
using FpvBattle.Domain;
using Microsoft.EntityFrameworkCore;

namespace FpvBattle.Application.Features.Achievements.Collection;

public class UniversalSoldierAchievement : IAchievementAfterTimeUpdate
{
    private readonly IApplicationDbContext _db;

    public UniversalSoldierAchievement(IApplicationDbContext db)
    {
        _db = db;
    }

    public string Name => "UniversalSoldier";
    public string Title => "Universal Soldier";
    public string Description => "Participate in both classes in a single day";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
            return false;

        var cupsFlownToday = await _db.TrackTimeDeltas
            .Where(d => d.PilotId == pilot.Id && d.Competition.State == CompetitionState.Started)
            .Select(d => d.Competition.CupId)
            .Distinct()
            .ToListAsync();

        return cupsFlownToday.Contains(CupIds.OpenClass) && cupsFlownToday.Contains(CupIds.WhoopClass);
    }
}