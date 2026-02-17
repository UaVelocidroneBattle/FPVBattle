using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Collection;

public class FirstResultAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "FirstResult";
    public string Title => "Flash";
    public string Description => "First result of the day";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var firstResults = deltas.FirstOrDefault()?.Competition.TimeDeltas.Count == deltas.Count;

        if (!firstResults)
        {
            return false;
        }

        // If there are more than one delta then we don't know which one is the first result
        if (deltas.Count > 1)
        {
            return false;
        }

        return true;
    }
}
