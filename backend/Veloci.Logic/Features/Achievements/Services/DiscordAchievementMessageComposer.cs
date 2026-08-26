using System.Text;
using Veloci.Data.Domain;

namespace Veloci.Logic.Features.Achievements.Services;

public class DiscordAchievementMessageComposer
{
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
