using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Helpers;

namespace Veloci.Logic.Features.Achievements.Collection;

public class LateBirdAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "LateBird";
    public string Title => "Night Owl";
    public string Description => "Update time between 1 and 4 AM Kyiv time";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        const int hourFrom = 1;
        const int hourTo = 4;

        var ukraineTime = UkrainianHelper.GetCurrentKyivTime();

        return ukraineTime.Hour is >= hourFrom and < hourTo;
    }
}
