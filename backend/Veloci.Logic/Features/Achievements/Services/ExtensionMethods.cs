using Veloci.Data.Domain;
using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Services;

public static class ExtensionMethods
{
    public static void AddAchievement(this Pilot pilot, IAchievement achievement)
    {
        var pilotAchievement = new PilotAchievement { Pilot = pilot, Date = DateTime.Now, Name = achievement.Name };

        pilot.Achievements.Add(pilotAchievement);
    }
}
