using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Serilog;
using Veloci.Logic.Helpers;

using Poll = Discord.Poll;

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
        if (_client is null)
            return null;

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

    public async Task<ulong?> SendPollAsync(BotPoll poll)
    {
        if (_client is null)
            return null;

        try
        {
            EnsureChannelResolved();

            _log.Information("🗳️ Sending Discord poll to channel {ChannelName}: {Question}", _channelName, poll.Question);

            // For Discord.NET 3.18.0, we'll create a text-based poll with emoji reactions
            var pollMessage = poll.Question + "\n\n" +
                             "Vote by reacting with:\n" +
                             string.Join("\n", poll.Options.Select((option, index) => 
                                 $"{(char)('1' + index)}. **{option.Text}** ({GetEmojiForOption(index)}) "));

            var message = await _channel!.SendMessageAsync(pollMessage);
            
            // Add reactions for each option
            for (int i = 0; i < Math.Min(poll.Options.Count, 10); i++)
            {
                try
                {
                    var emoji = GetEmojiForOption(i);
                    await message.AddReactionAsync(emoji);
                }
                catch (Exception ex)
                {
                    _log.Warning(ex, "Failed to add reaction to poll message in channel {ChannelName}", _channelName);
                }
            }
            
            _log.Information("🗳️ Created Discord text-based poll in channel {ChannelName}, message ID: {MessageId}", _channelName, message.Id);
            return message.Id;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create Discord poll in channel {ChannelName}", _channelName);
            return null;
        }
    }

    public async Task<DiscordPollResult?> StopPollAsync(ulong messageId)
    {
        if (_client is null)
            return null;

        try
        {
            EnsureChannelResolved();

            _log.Debug("Stopping Discord poll with message ID {MessageId} in channel {ChannelName}", messageId, _channelName);

            var restMessage = await _channel!.GetMessageAsync(messageId);
            
            if (restMessage is not IUserMessage userMessage)
            {
                _log.Warning("Message {MessageId} is not a user message in channel {ChannelName}", messageId, _channelName);
                return null;
            }

            // Get all reactions from the message
            var reactions = userMessage.Reactions;
            var results = new DiscordPollResult
            {
                TotalVoterCount = reactions.Sum(r => r.ReactionCount - 1), // Subtract 1 for the bot's own reaction
                IsCompleted = true
            };

            // Map each reaction to option vote counts
            for (int i = 0; i < reactions.Count; i++)
            {
                var emoji = GetEmojiForOption(i);
                var reaction = reactions.FirstOrDefault(r => r.Key.ToString() == emoji.ToString());
                if (reaction.Key != null)
                {
                    results.OptionVoterCounts[emoji.ToString()] = reaction.ReactionCount - 1; // Subtract bot's reaction
                }
            }

            // Edit the message to indicate it's closed
            await userMessage.ModifyAsync(msg => msg.Content = msg.Content + "\n\n✅ **Poll closed**");

            _log.Information("🗳️ Stopped Discord poll {MessageId} with {VoterCount} voters in channel {ChannelName}", 
                messageId, results.TotalVoterCount, _channelName);
                
            return results;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to stop Discord poll {MessageId} in channel {ChannelName}", messageId, _channelName);
            return null;
        }
    }

    public async Task<ulong?> SendReplyAsync(string message, ulong replyToMessageId)
    {
        if (_client is null)
            return null;

        try
        {
            EnsureChannelResolved();

            _log.Debug("Sending Discord reply to message {MessageId} in channel {ChannelName}", replyToMessageId, _channelName);

            var reference = new MessageReference(replyToMessageId);
            var requestOptions = new RequestOptions
            {
                MessageReference = reference
            };
            
            var result = await _channel!.SendMessageAsync(message, requestOptions);

            _log.Debug("Sent Discord reply to message {MessageId} in channel {ChannelName}, reply ID: {ReplyId}", 
                replyToMessageId, _channelName, result.Id);
            
            return result.Id;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to send reply to message {MessageId} in channel {ChannelName}", replyToMessageId, _channelName);
            return null;
        }
    }

    /// <summary>
    /// Gets an emoji for a poll option index (0-9)
    /// </summary>
    private static IEmoji GetEmojiForOption(int index)
    {
        return index switch
        {
            0 => new Emoji("1️⃣"),
            1 => new Emoji("2️⃣"),
            2 => new Emoji("3️⃣"),
            3 => new Emoji("4️⃣"),
            4 => new Emoji("5️⃣"),
            5 => new Emoji("6️⃣"),
            6 => new Emoji("7️⃣"),
            7 => new Emoji("8️⃣"),
            8 => new Emoji("9️⃣"),
            9 => new Emoji("🔟"),
            _ => new Emoji("❓")
        };
    }
}
