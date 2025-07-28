using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot.Types;

namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public class TelegramCommandProcessor
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramCommandProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ProcessAsync(Message message)
    {
        var text = message.Text;
        var parsed = ParseMessage(text);

        if (parsed is null)
        {
            return;
        }

        Log.Information("⚙️ Processing Telegram command {Command} from user {UserId} with {ParameterCount} parameters",
            parsed.Command, message.From?.Id, parsed.Parameters?.Length ?? 0);

        var command = GetCommand(parsed.Command);

        if (command is null)
        {
            Log.Warning("Unknown command attempted: {Command} from user {UserId}", parsed.Command, message.From?.Id);
            return;
        }

        try
        {
            var result = await command.ExecuteAsync(parsed.Parameters);
            var messageId = await TelegramBot.ReplyMessageAsync(result, message.MessageId, message.Chat.Id.ToString());

            Log.Information("✅ Executed command {Command} successfully", parsed.Command);

            if (messageId.HasValue && command.RemoveMessageAfterDelay)
            {
                Log.Debug("Command {Command} scheduled for auto-removal in 60 seconds", parsed.Command);
                BackgroundJob.Schedule(() => TelegramBot.RemoveMessageAsync(messageId.Value, message.Chat.Id.ToString()), TimeSpan.FromSeconds(60));
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to execute command {Command} from user {UserId}", parsed.Command, message.From?.Id);
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
        Log.Debug("Parsed Telegram command {Command} with parameters: [{Parameters}]",
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

