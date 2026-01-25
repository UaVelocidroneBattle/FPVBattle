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
    private readonly ICupService _cupService;
    private readonly ITelegramMessenger _messenger;

    public TelegramUpdateHandler(
        CompetitionConductor competitionConductor,
        TelegramCommandProcessor commandProcessor,
        ICupContextResolver cupContextResolver,
        ICupService cupService,
        ITelegramMessenger messenger)
    {
        _competitionConductor = competitionConductor;
        _commandProcessor = commandProcessor;
        _cupContextResolver = cupContextResolver;
        _cupService = cupService;
        _messenger = messenger;
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
            if (cupId is null)
            {
                _log.Warning("Competition restart requested from unbound chat {ChatId}", chatId);
                return;
            }

            var channelId = _cupService.GetTelegramChannelId(cupId);
            if (string.IsNullOrEmpty(channelId))
            {
                _log.Warning("No Telegram channel configured for cup {CupId}, ignoring restart request", cupId);
                return;
            }

            // Only process if message is from the configured channel for this cup
            if (chatId != channelId)
            {
                _log.Debug("Ignoring restart request from non-channel chat {ChatId} for cup {CupId}", chatId, cupId);
                return;
            }

            _log.Information("Competition restart requested for cup {CupId} from chat {ChatId}", cupId, chatId);
            await _messenger.SendMessageAsync(channelId, "Добре 🫡");
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(cupId), new TimeSpan(0, 0, 5));

            return;
        }

        if (MessageParser.IsCompetitionStop(text))
        {
            if (cupId is null)
            {
                _log.Warning("Competition stop requested from unbound chat {ChatId}", chatId);
                return;
            }

            var channelId = _cupService.GetTelegramChannelId(cupId);
            if (string.IsNullOrEmpty(channelId))
            {
                _log.Warning("No Telegram channel configured for cup {CupId}, ignoring stop request", cupId);
                return;
            }

            // Only process if message is from the configured channel for this cup
            if (chatId != channelId)
            {
                _log.Debug("Ignoring stop request from non-channel chat {ChatId} for cup {CupId}", chatId, cupId);
                return;
            }

            _log.Information("Competition stop requested for cup {CupId} from chat {ChatId}", cupId, chatId);
            await _messenger.SendMessageAsync(channelId, "Добре 🫡");
            BackgroundJob.Enqueue(() => _competitionConductor.StopPollAsync(cupId));
            BackgroundJob.Schedule(() => _competitionConductor.StopAsync(cupId), new TimeSpan(0, 0, 10));
            BackgroundJob.Schedule(() => _competitionConductor.SeasonResultsAsync(), new TimeSpan(0, 0, 30));
            BackgroundJob.Schedule(() => _competitionConductor.StartNewAsync(cupId), new TimeSpan(0, 0, 45));

            return;
        }

        await _commandProcessor.ProcessAsync(message);
    }
}
