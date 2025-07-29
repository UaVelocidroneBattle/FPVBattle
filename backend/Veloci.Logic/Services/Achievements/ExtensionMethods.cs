using Veloci.Data.Domain;
using Veloci.Logic.Achievements.Base;

namespace Veloci.Logic.Services.Achievements;

public static class ExtensionMethods
{
    public static void AddAchievement(this Pilot pilot, IAchievement achievement)
    {
        var pilotAchievement = new PilotAchievement
        {
            Pilot = pilot,
            Date = DateTime.Now,
            Name = achievement.Name
        };

        pilot.Achievements.Add(pilotAchievement);
    }
}
