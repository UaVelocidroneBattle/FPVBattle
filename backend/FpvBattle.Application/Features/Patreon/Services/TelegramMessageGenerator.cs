using System.Text;
using FpvBattle.Application.Bot.Telegram;
using FpvBattle.Application.Features.Patreon.Models;
using FpvBattle.Domain;

namespace FpvBattle.Application.Features.Patreon.Services;

public static class TelegramMessageGenerator
{
    private static readonly string[] WelcomeMessages =
    [
        "✈️ {0} вирішив(ла) підтримати нашу команду і став патроном ({1}). Дякуємо! 🤝",
        "Привіт і величезна подяка, {0}, за те, що долучилися до наших патронів ({1})",
        "🎮 {0} підключився до нашої команди патронів! Дякуємо!",
    ];

    public static string WelcomeMessage(IEnumerable<PatreonSupporter> supporters)
    {
        return string.Join("\n", supporters.Select(s => WelcomeLine(s.Name, s.TierName)));
    }

    private static string WelcomeLine(string supporterName, string? tierName)
    {
        var messageIndex = Math.Abs(supporterName.GetHashCode()) % WelcomeMessages.Length;
        var template = WelcomeMessages[messageIndex];
        var formattedName = $"*{TelegramMarkdown.Escape(supporterName)}*";
        var formattedTier = string.IsNullOrEmpty(tierName) ? "Patreon" : tierName;

        return string.Format(template, formattedName, formattedTier);
    }

    public static string AccruedFreeziesMessage(List<AccruedPatronFreezies> accrued)
    {
        var message = new StringBuilder("❄️ Свої щомісячні заморозки за підтримку на Patreon отримали:\n\n");

        foreach (var entry in accrued.OrderByDescending(x => x.FreeziesAccrued))
        {
            message.Append($"*{TelegramMarkdown.Escape(entry.PilotName)}*: {entry.FreeziesAccrued} фрізів\n");
        }

        message.Append("\nВикористовуйте з розумом!\n");

        return message.ToString();
    }
}
