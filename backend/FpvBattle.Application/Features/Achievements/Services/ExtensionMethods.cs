using FpvBattle.Application.Features.Achievements.Base;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Achievements.Services;

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
