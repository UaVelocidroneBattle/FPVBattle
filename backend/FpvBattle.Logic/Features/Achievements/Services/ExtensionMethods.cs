using FpvBattle.Domain;
using FpvBattle.Logic.Features.Achievements.Base;

namespace FpvBattle.Logic.Features.Achievements.Services;

public static class ExtensionMethods
{
    public static void AddAchievement(this Pilot pilot, IAchievement achievement)
    {
        var pilotAchievement = new PilotAchievement
        {
            Pilot = pilot,
            Date = DateTime.UtcNow,
            Name = achievement.Name
        };

        pilot.Achievements.Add(pilotAchievement);
    }
}
