using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Achievements.Collection;

public class FirstResultAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "FirstResult";
    public string Title => "Флеш";
    public string Description => "Перший результат цього дня";
    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
            return false;

        var firstResults = deltas.FirstOrDefault()?.Competition.TimeDeltas.Count == deltas.Count;

        if (!firstResults)
            return false;

        // If there are more than one delta then we don't know which one is the first result
        if (deltas.Count > 1)
            return false;

        return true;
    }
}
