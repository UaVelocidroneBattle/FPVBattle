using FpvBattle.Application.Features.Achievements.Base;

namespace FpvBattle.Application.Features.Achievements.Services;

public class InAppAchievementMessageComposer
{
    public string Achievement(IAchievement achievement)
    {
        return $"You got a new achievement: \"{achievement.Title}\"";
    }
}
