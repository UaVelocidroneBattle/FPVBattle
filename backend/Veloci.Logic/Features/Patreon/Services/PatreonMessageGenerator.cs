using System;

namespace Veloci.Logic.Features.Patreon.Services;

public static class PatreonMessageGenerator
{
    private static readonly string[] WelcomeMessages =
    [
        "✈️ {0} вирішив(ла) підтримати нашу команду і став патроном ({1}). Дякуємо! 🤝",
        "👏 Бурхливі овації для {0}, нашого нового патрона! Подякували!",
        "Привіт і величезна подяка, {0}, за те, що долучилися до наших патронів ({1})",
        "🎮 {0} підключився до нашої команди патронів! Дякуємо!",
        "🔧 {0} тепер наш технічний спонсор на Patreon. Команда мрії! 🤝",
    ];

    public static string GenerateWelcomeMessage(string supporterName, string? tierName, bool useDiscordMarkdown = false)
    {
        var messageIndex = Math.Abs(supporterName.GetHashCode()) % WelcomeMessages.Length;
        var template = WelcomeMessages[messageIndex];
        var formattedName = useDiscordMarkdown ? $"**{supporterName}**" : $"*{supporterName}*";
        var formattedTier = string.IsNullOrEmpty(tierName) ? "Patreon" : tierName;

        return string.Format(template, formattedName, formattedTier);
    }
}
