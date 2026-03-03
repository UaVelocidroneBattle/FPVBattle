using Microsoft.EntityFrameworkCore;
using Veloci.Data.Domain;
using Veloci.Data.Repositories;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Features.Achievements.Collection;

public class UniversalSoldierAchievement : IAchievementAfterTimeUpdate
{
    private readonly IRepository<TrackTimeDelta> _timeDeltas;

    public UniversalSoldierAchievement(IRepository<TrackTimeDelta> timeDeltas)
    {
        _timeDeltas = timeDeltas;
    }

    public string Name => "UniversalSoldier";
    public string Title => "Universal Soldier";
    public string Description => "Participate in both classes in a single day";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
            return false;

        var cupsFlownToday = await _timeDeltas.GetAll()
            .Where(d => d.PilotId == pilot.Id && d.Competition.State == CompetitionState.Started)
            .Select(d => d.Competition.CupId)
            .Distinct()
            .ToListAsync();

        return cupsFlownToday.Contains(CupIds.OpenClass) && cupsFlownToday.Contains(CupIds.WhoopClass);
    }
}