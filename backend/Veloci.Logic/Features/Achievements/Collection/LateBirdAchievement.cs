using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Helpers;

namespace Veloci.Logic.Features.Achievements.Collection;

public class LateBirdAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "LateBird";
    public string Title => "Сова";
    public string Description => "Оновивити час на треку з 1:00 до 4:00 ночі за київським часом";

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
