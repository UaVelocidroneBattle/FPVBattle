using Hangfire;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Veloci.Logic.Bot.Telegram.Commands.Core;
using Veloci.Logic.Features.Cups;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

public interface ITelegramUpdateHandler
{
    Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
}

public class TelegramUpdateHandler : ITelegramUpdateHandler
{
    private static readonly ILogger _log = Log.ForContext<TelegramUpdateHandler>();

    private readonly CompetitionConductor _competitionConductor;
    private readonly TelegramCommandProcessor _commandProcessor;
    private readonly ICupContextResolver _cupContextResolver;

    public TelegramUpdateHandler(
        CompetitionConductor competitionConductor,
        TelegramCommandProcessor commandProcessor,
        ICupContextResolver cupContextResolver)
    {
        _competitionConductor = competitionConductor;
        _commandProcessor = commandProcessor;
        _cupContextResolver = cupContextResolver;
    }

    public async Task OnUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message ?? update.ChannelPost;

        if (message is null)
            return;

        var text = message.Text;

        if (string.IsNullOrEmpty(text))
            return;

        var chatId = message.Chat.Id.ToString();
        var cupId = _cupContextResolver.GetCupIdByChatId(chatId);

        // If message is from unbound chat, silently ignore non-command messages
        if (cupId is null && !text.StartsWith('/'))
        {
            _log.Debug("Received non-command message from unbound chat {ChatId}, ignoring", chatId);
            return;
        }

        if (MessageParser.IsCompetitionRestart(text))
        {
            if (!TelegramBot.IsMainChannelId(chatId))
                return;

            if (cupId is null)
            {
                _log.Warning("Competition restart requested from unbound chat {ChatId}", chatId);
                return;
            }

            _log.Information("Competition restart requested for cup {CupId} from chat {ChatId}", cupId, chatId);
            await TelegramBot.SendMessageAsync("Добре 🫡");
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(cupId), new TimeSpan(0, 0, 5));

            return;
        }

        if (MessageParser.IsCompetitionStop(text))
        {
            if (!TelegramBot.IsMainChannelId(chatId))
                return;

            if (cupId is null)
            {
                _log.Warning("Competition stop requested from unbound chat {ChatId}", chatId);
                return;
            }

            _log.Information("Competition stop requested for cup {CupId} from chat {ChatId}", cupId, chatId);
            await TelegramBot.SendMessageAsync("Добре 🫡");
            BackgroundJob.Enqueue(() => _competitionConductor.StopPollAsync(cupId));
            BackgroundJob.Schedule(() => _competitionConductor.StopAsync(cupId), new TimeSpan(0, 0, 10));
            BackgroundJob.Schedule(() => _competitionConductor.SeasonResultsAsync(), new TimeSpan(0, 0, 30));
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(cupId), new TimeSpan(0, 0, 45));

            return;
        }

        await _commandProcessor.ProcessAsync(message);
    }
}
