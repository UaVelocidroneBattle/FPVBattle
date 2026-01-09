using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class NanoBoostAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "NanoBoost";
    public string Title => "NanoBoost";
    public string Description => "Покращити результат на 0.01s або менше";
    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var pilotDelta = deltas.FirstOrDefault(d => d.Pilot.Name == pilot.Name);

        if (pilotDelta is null)
            throw new Exception($"Pilot {pilot.Name} not found in deltas");

        if (!pilotDelta.TimeChange.HasValue)
            return false;

        // value is in ms. So 0.01s == 10ms
        return pilotDelta.TimeChange.Value is < 0 and >= -10;
    }
}
