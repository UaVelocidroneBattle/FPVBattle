using Discord;
using Discord.WebSocket;
using Serilog;
using Veloci.Logic.Bot;
using Veloci.Logic.Helpers;

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

    private const int MaxMessageLength = 2000;

    public async Task<ulong?> SendMessageAsync(string message)
    {
        try
        {
            EnsureChannelResolved();

            var chunks = TextHelper.SplitIntoChunks(message, MaxMessageLength);

            _log.Information("💬 Sending Discord message to channel {ChannelName} ({ChunkCount} chunk(s)): {MessagePreview}...",
                _channelName, chunks.Count, message.Length > 50 ? message[..50] + "..." : message);

            ulong? lastId = null;

            foreach (var chunk in chunks)
            {
                var result = await _channel.SendMessageAsync(chunk);
                lastId = result.Id;
            }

            _log.Information("Sent Discord message to channel {ChannelName} (last id: {MessageId})", _channelName, lastId);
            return lastId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send Discord message to channel {ChannelName}", _channelName);
            return null;
        }
    }


public async Task EditMessageAsync(ulong messageId, string message)
    {
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

    public async Task<ulong?> SendPollAsync(BotPoll poll)
    {
        try
        {
            EnsureChannelResolved();

            _log.Information("Sending Discord poll to channel {ChannelName}: {Question} with {OptionCount} options",
                _channelName, poll.Question, poll.Options.Count);

            var pollProperties = new PollProperties
            {
                Question = new PollMediaProperties { Text = poll.Question },
                Answers = poll.Options.Select(opt => new PollMediaProperties { Text = opt.Text }).ToList(),
                Duration = 24,
                AllowMultiselect = false,
                LayoutType = PollLayout.Default
            };

            var result = await _channel!.SendMessageAsync(poll: pollProperties);

            _log.Information("Sent Discord poll to channel {ChannelName}, message ID: {MessageId}", _channelName, result.Id);
            return result.Id;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send poll to channel {ChannelName}", _channelName);
            return null;
        }
    }

    public async Task<BotPollResults?> StopPollAsync(ulong messageId)
    {
        try
        {
            EnsureChannelResolved();

            _log.Information("Stopping Discord poll {MessageId} in channel {ChannelName}", messageId, _channelName);

            var message = await _channel!.GetMessageAsync(messageId);

            if (message is not IUserMessage userMessage)
            {
                _log.Warning("Poll message {MessageId} not found or is not a user message in channel {ChannelName}", messageId, _channelName);
                return null;
            }

            await userMessage.EndPollAsync(null);

            var updatedMessage = await _channel.GetMessageAsync(messageId) as IUserMessage;
            var pollResults = updatedMessage?.Poll?.Results;

            if (pollResults is null)
            {
                _log.Warning("No poll results available for message {MessageId} after ending poll", messageId);
                return null;
            }

            var voteCounts = pollResults.Value.AnswerCounts
                .OrderBy(a => a.AnswerId)
                .Select(a => (int)a.Count)
                .ToList();

            _log.Information("Discord poll {MessageId} ended with vote counts: {Counts}",
                messageId, string.Join(", ", voteCounts));

            return new BotPollResults { VoteCounts = voteCounts };
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to stop poll {MessageId} in channel {ChannelName}", messageId, _channelName);
            return null;
        }
    }
}
