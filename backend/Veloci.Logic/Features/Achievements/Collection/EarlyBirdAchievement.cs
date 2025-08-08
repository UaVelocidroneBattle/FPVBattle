using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Helpers;

namespace Veloci.Logic.Features.Achievements.Collection;

public class EarlyBirdAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "EarlyBird";
    public string Title => "Ранній птах";
    public string Description => "Оновити час з 4 до 8 ранку за київським часом";

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        const int hourFrom = 4;
        const int hourTo = 8;

        var ukraineTime = UkrainianHelper.GetCurrentKyivTime();

        return ukraineTime.Hour is >= hourFrom and < hourTo;
    }
}
