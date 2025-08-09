using System;

namespace Veloci.Logic.Features.Patreon.Services;

public static class PatreonMessageGenerator
{
    private static readonly string[] WelcomeMessages =
    {
        "🎯 {0} перейшов(ла) з пілота на спонсора {1}! Дякуємо, що допомагаєш нам тримати сервери в польоті. Ти справжній MVP нашої спільноти! 💪",
        "✈️ Вау! {0} вирішив(ла) підтримати нашу команду на рівні {1}! Завдяки таким людям як ти, ми можемо продовжувати робити FPV Battle безкоштовним для всіх. Ти легенда! 🙌",
        "🚀 Аплодисменти! {0} став(ла) нашим патроном! Поки інші тільки літають, ти ще й допомагаєш всім іншим це робити. Справжній герой! 👏",
        "Привіт і величезна подяка, {0}, за те, що долучилися до наших патронів {1}! ✨ Саме ваша підтримка дозволяє нам утримувати сервери і зберігати спільноту безкоштовною.",
        "🎮 {0} підключився до нашої команди підтримки! Дякуємо, що допомагаєш нам залишатися безкоштовними для всіх пілотів. Ти наш справжній бекап! 🛡️",
        "🔧 {0} тепер наш технічний спонсор {1}! Поки ми програмуємо безкоштовно, ти допомагаєш оплачувати \"пальне\" для серверів. Команда мрії! 🤝",
        "🏆 Браво, {0}! Ти перейшов(ла) в лігу патронів FPV Battle! Твоя підтримка - це те, що дозволяє нам робити магію для всієї спільноти безкоштовно. Дякуємо від щирого серця! ❤️",
        "🚁 {0} активував(ла) режим \"підтримка спільноти\"! Завдяки таким як ти, наші сервери гудуть, а пілоти літають. Ти справжній wingman нашої команди! 🤜🤛"
    };

    public static string GenerateWelcomeMessage(string supporterName, string? tierName, bool useDiscordMarkdown = false)
    {
        var messageIndex = Math.Abs(supporterName.GetHashCode()) % WelcomeMessages.Length;
        var template = WelcomeMessages[messageIndex];
        var formattedName = useDiscordMarkdown ? $"**{supporterName}**" : $"*{supporterName}*";
        var formattedTier = string.IsNullOrEmpty(tierName) ? "Patreon" : tierName;
        
        return string.Format(template, formattedName, formattedTier);
    }
}