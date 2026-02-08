using Discord;
using Discord.WebSocket;
using Serilog;

namespace Veloci.Logic.Bot.Discord;

/// <summary>
/// Discord bot instance configured for a specific channel
/// </summary>
public class DiscordBotChannel : IDiscordBot
{
    private static readonly ILogger _log = Log.ForContext<DiscordBotChannel>();

    private readonly DiscordSocketClient _client;
    private readonly string _channelName;
    private ITextChannel? _channel;

    public string ChannelName => _channelName;

    public DiscordBotChannel(DiscordSocketClient client, string channelName)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _channelName = channelName ?? throw new ArgumentNullException(nameof(channelName));

        // Try to find channel immediately if client is ready
        if (_client.ConnectionState == ConnectionState.Connected)
        {
            ResolveChannel();
        }
    }

    private void ResolveChannel()
    {
        _channel = _client.Guilds
            .SelectMany(g => g.Channels)
            .OfType<ITextChannel>()
            .FirstOrDefault(c => c.Name == _channelName);

        if (_channel is null)
        {
            _log.Warning("Discord channel with name {ChannelName} not found", _channelName);
        }
        else
        {
            _log.Debug("Resolved Discord channel {ChannelName} (ID: {ChannelId})", _channelName, _channel.Id);
        }
    }

    private void EnsureChannelResolved()
    {
        if (_channel is null)
        {
            ResolveChannel();
        }

        if (_channel is null)
        {
            throw new InvalidOperationException($"Discord channel '{_channelName}' could not be resolved");
        }
    }

    public async Task<ulong?> SendMessageAsync(string message)
    {
        if (_client is null)
            return null;

        try
        {
            EnsureChannelResolved();

            _log.Information("💬 Sending Discord message to channel {ChannelName}: {MessagePreview}...",
                _channelName, message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            var result = await _channel!.SendMessageAsync(message);

            _log.Information("Sent Discord message {MessageId} to channel {ChannelName}", result.Id, _channelName);
            return result.Id;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send Discord message to channel {ChannelName}", _channelName);
            return null;
        }
    }

    public async Task EditMessageAsync(ulong messageId, string message)
    {
        if (_client is null)
            return;

        try
        {
            EnsureChannelResolved();

            _log.Debug("Editing Discord message {MessageId} in channel {ChannelName}", messageId, _channelName);

            var messageToEdit = await _channel!.GetMessageAsync(messageId);

            if (messageToEdit is IUserMessage userMessage)
            {
                await userMessage.ModifyAsync(m => m.Content = message);
                _log.Debug("Edited Discord message {MessageId} successfully", messageId);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to edit Discord message {MessageId} in channel {ChannelName}", messageId, _channelName);
        }
    }

    public async Task SendMessageInThreadAsync(ulong messageId, string threadName, string message)
    {
        if (_client is null)
            return;

        try
        {
            EnsureChannelResolved();

            var sourceMessage = await _channel!.GetMessageAsync(messageId);

            if (sourceMessage is null)
            {
                _log.Warning("Source message {MessageId} not found in channel {ChannelName}", messageId, _channelName);
                return;
            }

            var threads = await _channel.GetActiveThreadsAsync();
            var thread = threads.FirstOrDefault(t => t.Name == threadName);

            if (thread is null)
            {
                _log.Debug("Creating new thread {ThreadName} from message {MessageId} in channel {ChannelName}",
                    threadName, messageId, _channelName);
                thread = await _channel.CreateThreadAsync(threadName, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, sourceMessage);
            }

            _log.Debug("Sending message to thread {ThreadName} in channel {ChannelName}", threadName, _channelName);
            await thread.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send message to thread {ThreadName} in channel {ChannelName}", threadName, _channelName);
        }
    }

    public async Task ArchiveThreadAsync(string threadName)
    {
        if (_client is null)
            return;

        try
        {
            EnsureChannelResolved();

            var threads = await _channel!.GetActiveThreadsAsync();
            var thread = threads.FirstOrDefault(t => t.Name == threadName);

            if (thread is not null)
            {
                _log.Debug("Archiving thread {ThreadName} in channel {ChannelName}", threadName, _channelName);
                await thread.ModifyAsync(t => t.Archived = true);
            }
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to archive thread {ThreadName} in channel {ChannelName}", threadName, _channelName);
        }
    }

    public async Task ChangeChannelTopicAsync(string message)
    {
        if (_client is null)
            return;

        try
        {
            EnsureChannelResolved();

            _log.Debug("Changing topic for channel {ChannelName} to: {Topic}", _channelName, message);
            await _channel!.ModifyAsync(c => c.Topic = message);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to change topic for channel {ChannelName}", _channelName);
        }
    }

    public async Task SendImageAsync(byte[] imageBytes, string imageName)
    {
        if (_client is null)
            return;

        try
        {
            EnsureChannelResolved();

            _log.Information("🖼️ Sending Discord image {ImageName} to channel {ChannelName}", imageName, _channelName);

            using var stream = new MemoryStream(imageBytes);
            await _channel!.SendFileAsync(stream, imageName);

            _log.Debug("Sent Discord image {ImageName} to channel {ChannelName}", imageName, _channelName);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send image {ImageName} to channel {ChannelName}", imageName, _channelName);
        }
    }
}
