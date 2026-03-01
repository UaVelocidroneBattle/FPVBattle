using System.Text;
using Veloci.Logic.Features.Patreon.Models;

namespace Veloci.Logic.Features.Patreon.Services;

public static class DiscordMessageGenerator
{
    private static readonly string[] WelcomeMessages =
    [
        "✈️ {0} decided to support our team and became a patron ({1}). Thank you! 🤝",
        "Hello and huge thank you, {0}, for joining our patrons ({1}) 🙌",
        "🎮 {0} joined our patron squad! Thank you!",
    ];

    public static string WelcomeMessage(string supporterName, string? tierName)
    {
        var messageIndex = Math.Abs(supporterName.GetHashCode()) % WelcomeMessages.Length;
        var template = WelcomeMessages[messageIndex];
        var formattedName = $"**{supporterName}**";
        var formattedTier = string.IsNullOrEmpty(tierName) ? "Patreon" : tierName;

        return string.Format(template, formattedName, formattedTier);
    }

    public static string AccruedFreeziesMessage(List<AccruedPatronFreezies> accrued)
    {
        var message = new StringBuilder("❄️ The following pilots received their monthly freezies for supporting us on Patreon:\n\n");

        foreach (var entry in accrued.OrderByDescending(x => x.FreeziesAccrued))
        {
            message.Append($"**{entry.PilotName}**: {entry.FreeziesAccrued} freezies\n");
        }

        message.Append("\nUse them wisely!\n");

        return message.ToString();
    }
}
