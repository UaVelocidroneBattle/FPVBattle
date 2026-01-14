using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class BeastAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "Beast";
    public string Title => "Звір";
    public string Description => "Пролетіти трек за 66.6s";
    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var pilotDelta = deltas.FirstOrDefault(d => d.Pilot.Name == pilot.Name);

        if (pilotDelta is null)
            throw new Exception($"Pilot {pilot.Name} not found in deltas");

        // value is in ms. So 66.6s == 66600ms
        return pilotDelta.TrackTime == 66600;
    }
}
