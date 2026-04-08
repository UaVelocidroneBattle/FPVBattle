using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Services;

namespace Veloci.Logic.Features.Achievements.Collection;

public class EarlyBirdAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "EarlyBird";
    public string Title => "Early Bird";
    public string Description => "Update time between 4 and 8 AM in your local time";
    public string? CupId => null;

    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var updateTime = deltas.FirstOrDefault()?.Date;

        if (updateTime is null)
        {
            return false;
        }

        const int hourFrom = 4;
        const int hourTo = 8;

        var pilotLocalTime = TimeZoneService.GetLocalTime(updateTime.Value, pilot.Country);

        return pilotLocalTime.Hour is >= hourFrom and < hourTo;
    }
}
