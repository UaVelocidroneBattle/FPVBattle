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
        Log.Error(exception, "Error in telegram bot");
    }

    public static async Task SendMessageAsync(string message)
    {
        try
        {
            var result = await _client.SendTextMessageAsync(
                chatId: _channelId,
                text: Isolate(message),
                parseMode: ParseMode.MarkdownV2);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to send a message '{Message}'", message);
        }
    }

    public static async Task<int?> ReplyMessageAsync(string message, int messageId, string chatId)
    {
        try
        {
            var result = await _client.SendTextMessageAsync(
                chatId: chatId,
                replyToMessageId: messageId,
                parseMode: ParseMode.MarkdownV2,
                text: Isolate(message));

            return result.MessageId;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to send a message '{Message}'", message);
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
            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                caption: message,
                photo: new InputFileUrl(fileUrl)
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to send a photo");
        }
    }

    public static async Task SendPhotoAsync(Stream file, string? message = null)
    {
        file.Position = 0; // Weird fix. It throws an exception without

        if (message is not null)
            message = Isolate(message);

        try
        {
            var result = await _client.SendPhotoAsync(
                chatId: _channelId,
                photo: new InputFileStream(file),
                caption: message
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to send a photo");
        }
    }

    public static async Task<int?> SendPollAsync(BotPoll poll)
    {
        try
        {
            var message = await _client.SendPollAsync(
                chatId: _channelId,
                question: poll.Question,
                options: poll.Options.Select(x => x.Text)
            );

            return message.MessageId;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to send a poll");
            return null;
        }
    }

    public static async Task<Poll?> StopPollAsync(int messageId)
    {
        try
        {
            return await _client.StopPollAsync(_channelId, messageId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to stop the poll");
            return null;
        }
    }

    public static async Task RemoveMessageAsync(int messageId, string chatId)
    {
        try
        {
            await _client.DeleteMessageAsync(chatId, messageId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Telegram. Failed to remove the message");
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
        .Replace("`", "\\`")
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
