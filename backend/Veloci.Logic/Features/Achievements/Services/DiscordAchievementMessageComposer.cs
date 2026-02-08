using System.Text;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Services;

public class DiscordAchievementMessageComposer
{
    public string DayStreakAchievement(Pilot pilot)
    {
        return pilot.DayStreak switch
        {
            10 or 20 => $"**{pilot.Name}** already has a **{pilot.DayStreak}** day streak",
            50 => $"**{pilot.Name}** reached a **{pilot.DayStreak}** day streak",
            75 => $"**{pilot.Name}** is holding a **{pilot.DayStreak}** day streak",
            100 => $"**{pilot.Name}** conquered a **{pilot.DayStreak}** day streak",
            150 => $"**{pilot.Name}** crossed a **{pilot.DayStreak}** day streak",
            200 => $"**{pilot.Name}** has an incredible **{pilot.DayStreak}** day streak",
            250 => $"**{pilot.Name}** already has a **{pilot.DayStreak}** day streak",
            300 => $"**{pilot.Name}** reached an impressive **{pilot.DayStreak}** day streak",
            365 => $"**{pilot.Name}** celebrates a **{pilot.DayStreak}** day streak. A whole year!",
            500 => $"**{pilot.Name}** conquered a **{pilot.DayStreak}** day streak. Wow!",
            1000 => $"**{pilot.Name}** has an astounding **{pilot.DayStreak}** day streak",
            _ => string.Empty
        };
    }

    public string AchievementList(AchievementCheckResults results)
    {
        if (!results.Any())
        {
            return string.Empty;
        }

        var message = new StringBuilder($"### 🚀 New achievements:{Environment.NewLine}{Environment.NewLine}");

        foreach (var result in results)
        {
            message.AppendLine(
                $"**{result.Pilot.Name}** → 🎖 {result.Achievement.Title} ({result.Achievement.Description})");
        }

        return message.ToString();
    }
}
