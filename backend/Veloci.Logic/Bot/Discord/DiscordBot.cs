using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Veloci.Logic.Bot.Discord;

public interface IDiscordBot
{
    Task<ulong?> SendMessageAsync(string message);
    Task EditMessageAsync(ulong messageId, string message);
    Task SendMessageInThreadAsync(ulong messageId, string threadName, string message);
    Task ArchiveThreadAsync(string threadName);
    Task ChangeChannelTopicAsync(string message);
    Task SendImageAsync(byte[] imageBytes);
}

public class DiscordBot : IDiscordBot
{
    private static readonly ILogger _log = Serilog.Log.ForContext<DiscordBot>();
    
    private DiscordSocketClient? _client;
    private readonly string? _token;
    private readonly string? _channelName;
    private ITextChannel? _channel;

    public DiscordBot(IConfiguration configuration)
    {
        _token = configuration.GetSection("Discord:BotToken").Value;
        _channelName = configuration.GetSection("Discord:Channel").Value;

        _log.Debug("Discord channel: {@channel}", _channel);
    }

    #region Configuration section

    public async Task StartAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            _log.Information("Discord is disabled, because token is empty");
            return;
        }

        _client = new DiscordSocketClient();
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();

        _client.Ready += OnBotReady;
    }

    private Task OnBotReady()
    {
        _channel = _client.Guilds
            .SelectMany(g => g.Channels)
            .OfType<ITextChannel>()
            .FirstOrDefault(c => c.Name == _channelName);

        if (_channel is null)
        {
            _log.Error("Discord channel with name {ChannelName} not found.", _channelName);
            return Task.CompletedTask;
        }

        _log.Information("Discord bot is ready.");
        return Task.CompletedTask;
    }

    private static Task Log(LogMessage msg)
    {
        _log.Verbose(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        if (_client is null)
            return;

        await _client.StopAsync();
    }

    #endregion

    public async Task<ulong?> SendMessageAsync(string message)
    {
        if (_client is null || _channel is null)
            return null;

        try
        {
            _log.Information("💬 Sending Discord message to channel {ChannelName}: {MessagePreview}...", 
                _channel.Name, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            var result = await _channel.SendMessageAsync(message);
            
            _log.Information("Sent Discord message {MessageId} to channel {ChannelName}", result.Id, _channel.Name);
            return result.Id;
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to send message. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
            return null;
        }
    }

    public async Task EditMessageAsync(ulong messageId, string message)
    {
        if (_client is null || _channel is null)
            return;

        var messageToEdit = await _channel.GetMessageAsync(messageId);

        if (messageToEdit is null)
        {
            _log.Warning("Message with ID {MessageId} not found in channel {Channel}", messageId, _channel.Name);
            return;
        }

        try
        {
            _log.Information("Editing Discord message {MessageId} in channel {ChannelName}: {MessagePreview}...", 
                messageId, _channel.Name, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            await _channel.ModifyMessageAsync(messageId, x =>
            {
                x.Content = message;
            });
            
            _log.Debug("Discord message {MessageId} edited successfully", messageId);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to edit message. Guild: {Guild}, Channel: {Channel}, MessageId: {MessageId}", _channel.Guild.Name, _channel.Name, messageId);
        }
    }

    public async Task SendMessageInThreadAsync(ulong messageId, string threadName, string message)
    {
        if (_client is null || _channel is null)
            return;

        var allThreads = await _channel.GetActiveThreadsAsync();
        var thread = allThreads.FirstOrDefault(thread => thread.Name == threadName);
        var messageToReply = await _channel.GetMessageAsync(messageId);

        try
        {
            if (thread is null)
            {
                _log.Information("🧵 Creating Discord thread {ThreadName} for message {MessageId}", threadName, messageId);
                thread = await _channel.CreateThreadAsync(threadName, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, messageToReply);
                _log.Debug("Discord thread {ThreadName} created successfully", threadName);
            }
            else
            {
                _log.Debug("Using existing Discord thread {ThreadName}", threadName);
            }

            _log.Information("Sending message to Discord thread {ThreadName}: {MessagePreview}...", 
                threadName, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            await thread.SendMessageAsync(message);
            _log.Debug("Message sent successfully to Discord thread {ThreadName}", threadName);

        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to send a message in a thread. Guild: {Guild}, Channel: {Channel}, ThreadName: {ThreadName}", _channel.Guild.Name, _channel.Name, threadName);
        }
    }

    public async Task ArchiveThreadAsync(string threadName)
    {
        if (_client is null || _channel is null)
            return;

        var allThreads = await _channel.GetActiveThreadsAsync();
        var thread = allThreads.FirstOrDefault(thread => thread.Name == threadName);

        if (thread is null)
        {
            _log.Warning("Thread with name {ThreadName} not found in channel {Channel}", threadName, _channel.Name);
            return;
        }

        try
        {
            _log.Information("Archiving Discord thread {ThreadName}", threadName);
            await thread.ModifyAsync(x => x.Archived = true);
            _log.Debug("Discord thread {ThreadName} archived successfully", threadName);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to archive thread. Guild: {Guild}, Channel: {Channel}, ThreadName: {ThreadName}", _channel.Guild.Name, _channel.Name, threadName);
        }
    }

    public async Task ChangeChannelTopicAsync(string message)
    {
        if (_client is null || _channel is null)
            return;

        try
        {
            _log.Information("Changing Discord channel {ChannelName} topic to: {Topic}", _channel.Name, message);
            await _channel.ModifyAsync(x =>
            {
                x.Topic = message;
            });
            _log.Debug("Discord channel topic changed successfully");
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to change channel topic. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
        }
    }

    public async Task SendImageAsync(byte[] imageBytes)
    {
        if (_client is null || _channel is null)
            return;

        try
        {
            _log.Information("🖼️ Sending Discord image to channel {ChannelName} ({ImageSize} bytes)", _channel.Name, imageBytes.Length);
            var result = await _channel.SendFileAsync(new MemoryStream(imageBytes), "winners");
            _log.Information("Discord image sent successfully as message {MessageId}", result.Id);
        }
        catch (Exception e)
        {
            _log.Error(e, "Failed to send an image. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
        }
    }
}
