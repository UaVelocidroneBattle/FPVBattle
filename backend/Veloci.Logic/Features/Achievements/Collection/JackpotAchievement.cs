using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;
using Veloci.Logic.Helpers;

namespace Veloci.Logic.Features.Achievements.Collection;

public class JackpotAchievement : IAchievementAfterTimeUpdate
{
    public string Name => "Jackpot";
    public string Title => "Джекпот";
    public string Description => "Встановити час з однакових цифр (напр. 77.777)";
    public async Task<bool> CheckAsync(Pilot pilot, List<TrackTimeDelta> deltas)
    {
        if (pilot.HasAchievement(Name))
        {
            return false;
        }

        var pilotDelta = deltas.FirstOrDefault(d => d.Pilot.Name == pilot.Name);

        if (pilotDelta is null)
            throw new Exception($"Pilot {pilot.Name} not found in deltas");

        var pilotTime = pilotDelta.TrackTime;
        var timeString = TrackTimeConverter.MsToSec(pilotTime);

        if (!timeString.Contains('.'))
        {
            return false;
        }

        var parts = timeString.Split('.');

        if (parts.Length != 2 || parts[1].Length != 3)
        {
            return false;
        }

        var digitsOnly = timeString.Replace(".", "");

        return digitsOnly.Length > 0 && digitsOnly.All(c => c == digitsOnly[0]);
    }
}
