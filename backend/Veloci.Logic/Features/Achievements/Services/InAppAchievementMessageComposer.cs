using Veloci.Logic.Features.Achievements.Base;

namespace Veloci.Logic.Features.Achievements.Services;

public class InAppAchievementMessageComposer
{
    public string Achievement(IAchievement achievement)
    {
        return $"You got a new achievement: \"{achievement.Title}\"";
    }
}
