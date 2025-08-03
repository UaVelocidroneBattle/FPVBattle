using Hangfire;
using Telegram.Bot;
using Telegram.Bot.Types;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

public interface ITelegramUpdateHandler
{
    Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}

public class TelegramUpdateHandler : ITelegramUpdateHandler
{
    private readonly CompetitionConductor _competitionConductor;
    private readonly TelegramCommandProcessor _commandProcessor;

    public TelegramUpdateHandler(
        CompetitionConductor competitionConductor,
        TelegramCommandProcessor commandProcessor)
    {
        _competitionConductor = competitionConductor;
        _commandProcessor = commandProcessor;
    }

    public async Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message ?? update.ChannelPost;

        if (message is null)
            return;

        var text = message.Text;

        if (string.IsNullOrEmpty(text))
            return;

        if (MessageParser.IsCompetitionRestart(text))
        {
            if (!TelegramBot.IsMainChannelId(message.Chat.Id.ToString()))
                return;

            await TelegramBot.SendMessageAsync("Добре 🫡");
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(), new TimeSpan(0, 0, 5));

            return;
        }

        if (MessageParser.IsCompetitionStop(text))
        {
            if (!TelegramBot.IsMainChannelId(message.Chat.Id.ToString()))
                return;

            await TelegramBot.SendMessageAsync("Добре 🫡");
            BackgroundJob.Enqueue(() => _competitionConductor.StopPollAsync());
            BackgroundJob.Schedule(() => _competitionConductor.StopAsync(), new TimeSpan(0, 0, 10));
            BackgroundJob.Schedule(() => _competitionConductor.SeasonResultsAsync(), new TimeSpan(0, 0, 30));
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(), new TimeSpan(0, 0, 45));

            return;
        }

        await _commandProcessor.ProcessAsync(message);
    }
}
