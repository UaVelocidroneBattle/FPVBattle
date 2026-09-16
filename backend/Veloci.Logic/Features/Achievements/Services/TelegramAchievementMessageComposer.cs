using System.Text;
using Veloci.Data.Domain;
using Veloci.Logic.Bot.Telegram;

namespace Veloci.Logic.Features.Achievements.Services;

public class TelegramAchievementMessageComposer
{
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
                $"*{TelegramMarkdown.Escape(result.Pilot.Name)}* → 🎖 {result.Achievement.Title} ({result.Achievement.Description})");
        }

        return message.ToString();
    }
}
