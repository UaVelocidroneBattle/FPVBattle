using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot.Types;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public class TelegramCommandProcessor
{
    private static readonly ILogger _log = Log.ForContext<TelegramCommandProcessor>();

    private readonly IServiceProvider _serviceProvider;
    private readonly ICupContextResolver _cupContextResolver;

    public TelegramCommandProcessor(IServiceProvider serviceProvider, ICupContextResolver cupContextResolver)
    {
        _serviceProvider = serviceProvider;
        _cupContextResolver = cupContextResolver;
    }

    public async Task ProcessAsync(Message message)
    {
        var text = message.Text;
        var parsed = ParseMessage(text);

        if (parsed is null)
        {
            return;
        }

        var chatId = message.Chat.Id.ToString();
        var cupId = _cupContextResolver.GetCupIdByChatId(chatId);

        if (cupId != null)
        {
            _log.Information("⚙️ Processing Telegram command {Command} from user {UserId} in cup {CupId} with {ParameterCount} parameters",
                parsed.Command, message.From?.Id, cupId, parsed.Parameters?.Length ?? 0);
        }
        else
        {
            _log.Information("⚙️ Processing Telegram command {Command} from user {UserId} in unbound chat with {ParameterCount} parameters",
                parsed.Command, message.From?.Id, parsed.Parameters?.Length ?? 0);
        }

        var command = GetCommand(parsed.Command);

        if (command is null)
        {
            _log.Warning("Unknown command attempted: {Command} from user {UserId}", parsed.Command, message.From?.Id);
            return;
        }

        try
        {
            // Create command context with cup resolution
            var context = new TelegramCommandContext
            {
                ChatId = chatId,
                CupId = cupId,
                Parameters = parsed.Parameters
            };

            var result = await command.ExecuteAsync(context);
            var messageId = await TelegramBot.ReplyMessageAsync(result, message.MessageId, chatId);

            _log.Information("✅ Executed command {Command} successfully", parsed.Command);

            if (messageId.HasValue && command.RemoveMessageAfterDelay)
            {
                _log.Debug("Command {Command} scheduled for auto-removal in 60 seconds", parsed.Command);
                BackgroundJob.Schedule(() => TelegramBot.RemoveMessageAsync(messageId.Value, chatId), TimeSpan.FromSeconds(60));
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to execute command {Command} from user {UserId}", parsed.Command, message.From?.Id);
        }
    }

    private ParsedMessage? ParseMessage(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var split = text.Split(' ');
        var command = split.First();

        if (!command.StartsWith('/'))
            return null;

        var parameters = split.Skip(1).ToArray();
        _log.Debug("Parsed Telegram command {Command} with parameters: [{Parameters}]",
            command.ToLower(), string.Join(", ", parameters));

        return new ParsedMessage
        {
            Command = command.ToLower(),
            Parameters = parameters
        };
    }

    private ITelegramCommand? GetCommand(string command)
    {
        var availableCommands = _serviceProvider.GetServices<ITelegramCommand>().ToList();
        return availableCommands.FirstOrDefault(c => c.Keywords.Contains(command));
    }
}

public class ParsedMessage
{
    public required string Command { get; set; }
    public string[]? Parameters { get; set; }
}
