using System.Text;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Services;

public class TelegramAchievementMessageComposer
{
    public string DayStreakAchievement(Pilot pilot)
    {
        return pilot.DayStreak switch
        {
            10 or 20 => $"🎉 *{pilot.Name}* має вже *{pilot.DayStreak}* day streak",
            50 => $"🎉 *{pilot.Name}* досягнув *{pilot.DayStreak}* day streak",
            75 => $"🎉 *{pilot.Name}* тримає *{pilot.DayStreak}* day streak",
            100 => $"🎉 *{pilot.Name}* подолав *{pilot.DayStreak}* day streak",
            150 => $"🎉 *{pilot.Name}* перетнув *{pilot.DayStreak}* day streak",
            200 => $"🎉 *{pilot.Name}* має неймовірні *{pilot.DayStreak}* day streak",
            250 => $"🎉 *{pilot.Name}* має вже *{pilot.DayStreak}* day streak",
            300 => $"🎉 *{pilot.Name}* досягнув вражаючих *{pilot.DayStreak}* day streak",
            365 => $"🎉 *{pilot.Name}* відзначає *{pilot.DayStreak}* day streak. Цілий рік!",
            500 => $"🎉 *{pilot.Name}* подолав *{pilot.DayStreak}* day streak. Це вау!",
            1000 => $"🎉 *{pilot.Name}* має вражаючі *{pilot.DayStreak}* day streak",
            _ => string.Empty
        };
    }

    public string AchievementList(AchievementCheckResults results)
    {
        if (!results.Any())
        {
            return string.Empty;
        }

        var message = new StringBuilder($"🚀 *Нові ачівменти:*{Environment.NewLine}{Environment.NewLine}");

        foreach (var result in results)
        {
            message.AppendLine(
                $"*{result.Pilot.Name}* → 🎖 {result.Achievement.Title} ({result.Achievement.Description})");
        }

        return message.ToString();
    }
}
