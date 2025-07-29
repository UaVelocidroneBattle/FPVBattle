using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Veloci.Logic.Services;

namespace Veloci.Logic.Bot.Telegram;

public class TelegramBot
{
    private static readonly ILogger _log = Log.ForContext<TelegramBot>();
    
    private readonly IServiceProvider _sp;
    private static string _botToken;
    private static string _channelId;
    private static TelegramBotClient _client;
    private static CompetitionService _competitionService;
    private CancellationTokenSource _cts;

    public TelegramBot(IConfiguration configuration, IServiceProvider sp)
    {
        _sp = sp;
        _botToken = configuration.GetSection("Telegram:BotToken").Value;
        _channelId = configuration.GetSection("Telegram:ChannelId").Value;
    }

    public void Init()
    {
        if (string.IsNullOrEmpty(_botToken)) return;

        _client = new TelegramBotClient(_botToken);
        _cts = new CancellationTokenSource();
        var cancellationToken = _cts.Token;

        StartReceiving(cancellationToken);
    }

    private void StartReceiving(CancellationToken ct)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // receive all update types
        };

        _client.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            ct
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        var updater = scope.ServiceProvider.GetRequiredService<ITelegramUpdateHandler>();
        await updater.OnUpdateAsync(botClient, update, cancellationToken);
    }

    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        _log.Error(exception, "Error in telegram bot");
    }

    public static async Task SendMessageAsync(string message)
    {
        try
        {
            _log.Information("📲 Sending Telegram message to channel: {MessagePreview}...", 
                message.Length > 50 ? message.Substring(0, 50) + "..." : message);
            
            var result = await _client.SendTextMessageAsync(
                chatId: _channelId,
                text: Isolate(message),
                parseMode: ParseMode.MarkdownV2);
                
            _log.Debug("Telegram message sent successfully with {MessageLength} characters", message.Length);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}'", message);
        }
    }

    public static async Task<int?> ReplyMessageAsync(string message, int messageId, string chatId)
    {
        try
        {
            _log.Information("Sending Telegram reply to message {MessageId} in chat {ChatId}: {MessagePreview}...", 
                messageId, chatId, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
            
            var result = await _client.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: messageId,
                parseMode: ParseMode.MarkdownV2,
                text: Isolate(message));

            _log.Debug("Telegram reply sent successfully as message {ReplyMessageId}", result.MessageId);
            return result.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a message '{Message}'", message);
            return null;
        }
    }

    public static async Task<int?> ReplyMessageAsync(string message, int messageId)
    {
        return await ReplyMessageAsync(message, messageId, _channelId);
    }

    public static async Task SendPhotoAsync(string fileUrl, string? message = null)
    {
        if (message is not null)
            message = Isolate(message);

        try
        {
            _log.Information("🖼️ Sending Telegram photo from URL {PhotoUrl} with caption: {Caption}", 
                fileUrl, message ?? "(no caption)");
                
            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                caption: message,
                photo: new InputFileUrl(fileUrl)
            );
            
            _log.Debug("Telegram photo sent successfully as message {MessageId}", result.MessageId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from URL {PhotoUrl}", fileUrl);
        }
    }

    public static async Task SendPhotoAsync(Stream file, string? message = null)
    {
        file.Position = 0; // Weird fix. It throws an exception without

        if (message is not null)
            message = Isolate(message);

        try
        {
            _log.Information("Sending Telegram photo from stream ({FileSize} bytes) with caption: {Caption}", 
                file.Length, message ?? "(no caption)");
                
            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                photo: new InputFileStream(file),
                caption: message
            );
            
            _log.Debug("Telegram photo from stream sent successfully as message {MessageId}", result.MessageId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a photo from stream");
        }
    }

    public static async Task<int?> SendPollAsync(BotPoll poll)
    {
        try
        {
            _log.Information("🗳️ Sending Telegram poll: {Question} with {OptionCount} options", 
                poll.Question, poll.Options.Count());
                
            var message = await _client.SendPollAsync(
                chatId: _channelId,
                question: poll.Question,
                options: poll.Options.Select(x => x.Text)
            );

            _log.Information("Telegram poll sent successfully as message {MessageId}", message.MessageId);
            return message.MessageId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to send a poll with question '{Question}'", poll.Question);
            return null;
        }
    }

    public static async Task<Poll?> StopPollAsync(int messageId)
    {
        try
        {
            _log.Information("Stopping Telegram poll with message ID {MessageId}", messageId);
            var result = await _client.StopPollAsync(_channelId, messageId);
            
            if (result != null)
            {
                _log.Information("Telegram poll {MessageId} stopped successfully with {VoterCount} total votes", 
                    messageId, result.TotalVoterCount);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to stop the poll with message ID {MessageId}", messageId);
            return null;
        }
    }

    public static async Task RemoveMessageAsync(int messageId, string chatId)
    {
        try
        {
            _log.Debug("Removing Telegram message {MessageId} from chat {ChatId}", messageId, chatId);
            await _client.DeleteMessageAsync(chatId, messageId);
            _log.Debug("Telegram message {MessageId} removed successfully", messageId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Telegram. Failed to remove message {MessageId} from chat {ChatId}", messageId, chatId);
        }
    }

    public static async Task RemoveMessageAsync(int messageId)
    {
        await RemoveMessageAsync(messageId, _channelId);
    }

    private static string Isolate(string message) => message
        .Replace(".", "\\.")
        .Replace("!", "\\!")
        .Replace("-", "\\-")
        .Replace("_", "\\_")
        .Replace(")", "\\)")
        .Replace("(", "\\(")
        .Replace("#", "\\#");

    public void Stop()
    {
        _cts.Cancel();
    }

    public static bool IsMainChannelId(string chatId)
    {
        return chatId == _channelId;
    }
}
