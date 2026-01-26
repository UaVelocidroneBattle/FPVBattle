using Serilog;
using System.IO;
using Veloci.Logic.Features.Cups;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// Sends Telegram messages to cups based on their configuration
/// </summary>
public class TelegramCupMessenger : ITelegramCupMessenger
{
    private static readonly ILogger _log = Log.ForContext<TelegramCupMessenger>();

    private readonly ICupService _cupService;
    private readonly ITelegramMessenger _messenger;

    public TelegramCupMessenger(ICupService cupService, ITelegramMessenger messenger)
    {
        _cupService = cupService;
        _messenger = messenger;
    }

    public Task SendMessageToCupAsync(string cupId, string message)
    {
        return SendMessageToCupsAsync([cupId], message);
    }

    public Task SendMessageToCupsAsync(IEnumerable<string> cupIds, string message)
    {
        return SendToCupsAsync(cupIds, channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public Task SendMessageToAllCupsAsync(string message)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendMessageAsync(channelId, message));
    }

    public Task SendPhotoToAllCupsAsync(string fileUrl, string? caption = null)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, fileUrl, caption));
    }

    public Task SendPhotoToAllCupsAsync(Stream file, string? caption = null)
    {
        return SendToAllCupsAsync(channelId => _messenger.SendPhotoAsync(channelId, file, caption));
    }

    private async Task SendToCupsAsync(IEnumerable<string> cupIds, Func<string, Task> sendAction)
    {
        foreach (var cupId in cupIds)
        {
            var channelId = _cupService.GetTelegramChannelId(cupId);

            if (string.IsNullOrEmpty(channelId))
            {
                _log.Warning("Cup {CupId} does not have Telegram channel configured, skipping", cupId);
                continue;
            }

            try
            {
                await sendAction(channelId);
                _log.Debug("Sent message to cup {CupId} Telegram channel {ChannelId}", cupId, channelId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to send message to cup {CupId} Telegram channel", cupId);
            }
        }
    }

    private async Task SendToAllCupsAsync(Func<string, Task> sendAction)
    {
        var enabledCupIds = _cupService.GetEnabledCupIds().ToList();
        await SendToCupsAsync(enabledCupIds, sendAction);
    }
}
