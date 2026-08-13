using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Bot.Telegram;

namespace Veloci.Logic.Features.Achievements.Services;

public class TelegramAchievementMessageComposer
{
    public string DayStreakAchievement(Pilot pilot)
    {
        return pilot.DayStreak switch
        {
            10 or 20 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* має вже *{pilot.DayStreak}* day streak",
            50 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* досягнув *{pilot.DayStreak}* day streak",
            75 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* тримає *{pilot.DayStreak}* day streak",
            100 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* подолав *{pilot.DayStreak}* day streak",
            150 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* перетнув *{pilot.DayStreak}* day streak",
            200 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* має неймовірні *{pilot.DayStreak}* day streak",
            250 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* має вже *{pilot.DayStreak}* day streak",
            300 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* досягнув вражаючих *{pilot.DayStreak}* day streak",
            365 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* відзначає *{pilot.DayStreak}* day streak. Цілий рік!",
            500 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* подолав *{pilot.DayStreak}* day streak. Це вау!",
            1000 => $"🎉 *{TelegramMarkdown.EscapeUserText(pilot.Name)}* має вражаючі *{pilot.DayStreak}* day streak",
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
                $"*{TelegramMarkdown.EscapeUserText(result.Pilot.Name)}* → 🎖 {result.Achievement.Title} ({result.Achievement.Description})");
        }

        return message.ToString();
    }
}
