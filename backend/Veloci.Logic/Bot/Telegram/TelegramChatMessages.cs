namespace Veloci.Logic.Bot.Telegram;

public class TelegramChatMessages : ChatMessages
{
    public TelegramChatMessages()
    {
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "👀 А де всі?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🧐 Є хто живий?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🫠 Трек сам себе не пролетить"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🙃 Може пора вже?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🙄 Чого чекаємо?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🤓 Запускайте вже ваші симулятори"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "😴 Zzzz..."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "😕 Знову світла немає?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "👀 Подвійний клік по іконці Velocidrone на вашому робочому столі, будь ласка"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "👀 Цілу годину нікого, шо у вас там за свято?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "📺 Може, ще кави поп’єте? І серіал глянете? Не спішіть, звісно."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "📉 Динаміка польотів — як курс гривні під час кризи."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "📵 Ви там що, Wi-Fi з глиняного горщика ловите?"));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🛋️ Може, хтось ще пледик піднесе? Комфорт понад усе, політ зачекає."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🧘‍♂️ Та не поспішайте, ніби медитація важливіша за FPV."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🪞 Подивіться в дзеркало. Там пілот, якому лінь літати."));
        Messages.Add(new ChatMessage(ChatMessageType.NobodyFlying, "🎻 Скрипка грає, трек чекає"));

        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "👀 А де всі інші?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "😐 Тільки один результат? Позорисько!"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🙄 Чого інші чекають?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🥱 Решта вирішила, що дивитись — цікавіше, ніж літати?"));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🤷‍♂️ Тільки один? А може й добре. Менше сорому в таблиці буде."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🥇 Ну що ж, золото автоматично. Дякуємо іншим за участь."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🫣 Решта вирішила зберегти самооцінку — і просто не стартувала."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🫥 Решта, певно, вийшла за хлібом."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🪦 Легенди кажуть, колись тут літало більше одного пілота..."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "🫡 Хтось мусив взяти відповідальність. Дякуємо, єдиний пілот."));
        Messages.Add(new ChatMessage(ChatMessageType.OnlyOneFlew, "😌 Ну хоч одному не лінь. Інші, схоже, чілять"));

        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "👌 Не забудь оцінити трек"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Оцінювати треки важливо 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Ну як тобі трек? 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Твоя думка важлива 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Оціни трек, якщо ще ні 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Йди голосуй 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "Чи є у вас 10 секунд на невеличке опитування? 👆"));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧐 Оцінити трек не важче, ніж в TikTok лайкнути. Спробуй."));
        Messages.Add(new ChatMessage(ChatMessageType.VoteReminder, "🧠 Навіть твоє пасивне «нормальний» — це теж фідбек."));
    }
}
