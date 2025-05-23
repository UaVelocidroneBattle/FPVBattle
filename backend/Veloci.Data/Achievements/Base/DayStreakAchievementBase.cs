using Veloci.Data.Domain;

namespace Veloci.Data.Achievements.Base;

public abstract class DayStreakAchievementBase : IAchievementPilotCheck
{
    public string Name => $"DayStreak{Days}";
    public string Title => $"{Days} day streak";

    protected abstract int Days { get; }

    public async Task<bool> CheckAsync(Pilot pilot)
    {
        if (pilot.HasAchievement(Name))
            return false;

        return pilot.MaxDayStreak >= Days;
    }
}
