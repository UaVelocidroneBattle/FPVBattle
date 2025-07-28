using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

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
    private DiscordSocketClient? _client;
    private readonly string? _token;
    private readonly string? _channelName;
    private ITextChannel? _channel;

    public DiscordBot(IConfiguration configuration)
    {
        _token = configuration.GetSection("Discord:BotToken").Value;
        _channelName = configuration.GetSection("Discord:Channel").Value;

        Serilog.Log.Debug("Discord channel: {@channel}", _channel);
    }

    #region Configuration section

    public async Task StartAsync()
    {
        if (string.IsNullOrEmpty(_token))
        {
            Serilog.Log.Information("Discord is disabled, because token is empty");
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
            Serilog.Log.Error("Discord channel with name {ChannelName} not found.", _channelName);
            return Task.CompletedTask;
        }

        Serilog.Log.Information("Discord bot is ready.");
        return Task.CompletedTask;
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Verbose(msg.ToString());
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
            Serilog.Log.Information("💬 Sending Discord message to channel {ChannelName}: {MessagePreview}...", 
                _channel.Name, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            var result = await _channel.SendMessageAsync(message);
            
            Serilog.Log.Information("Sent Discord message {MessageId} to channel {ChannelName}", result.Id, _channel.Name);
            return result.Id;
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to send message. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
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
            Serilog.Log.Warning("Message with ID {MessageId} not found in channel {Channel}", messageId, _channel.Name);
            return;
        }

        try
        {
            Serilog.Log.Information("Editing Discord message {MessageId} in channel {ChannelName}: {MessagePreview}...", 
                messageId, _channel.Name, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            await _channel.ModifyMessageAsync(messageId, x =>
            {
                x.Content = message;
            });
            
            Serilog.Log.Debug("Discord message {MessageId} edited successfully", messageId);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to edit message. Guild: {Guild}, Channel: {Channel}, MessageId: {MessageId}", _channel.Guild.Name, _channel.Name, messageId);
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
                Serilog.Log.Information("🧵 Creating Discord thread {ThreadName} for message {MessageId}", threadName, messageId);
                thread = await _channel.CreateThreadAsync(threadName, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, messageToReply);
                Serilog.Log.Debug("Discord thread {ThreadName} created successfully", threadName);
            }
            else
            {
                Serilog.Log.Debug("Using existing Discord thread {ThreadName}", threadName);
            }

            Serilog.Log.Information("Sending message to Discord thread {ThreadName}: {MessagePreview}...", 
                threadName, message.Length > 50 ? message.Substring(0, 50) + "..." : message);
                
            await thread.SendMessageAsync(message);
            Serilog.Log.Debug("Message sent successfully to Discord thread {ThreadName}", threadName);

        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to send a message in a thread. Guild: {Guild}, Channel: {Channel}, ThreadName: {ThreadName}", _channel.Guild.Name, _channel.Name, threadName);
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
            Serilog.Log.Warning("Thread with name {ThreadName} not found in channel {Channel}", threadName, _channel.Name);
            return;
        }

        try
        {
            Serilog.Log.Information("Archiving Discord thread {ThreadName}", threadName);
            await thread.ModifyAsync(x => x.Archived = true);
            Serilog.Log.Debug("Discord thread {ThreadName} archived successfully", threadName);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to archive thread. Guild: {Guild}, Channel: {Channel}, ThreadName: {ThreadName}", _channel.Guild.Name, _channel.Name, threadName);
        }
    }

    public async Task ChangeChannelTopicAsync(string message)
    {
        if (_client is null || _channel is null)
            return;

        try
        {
            Serilog.Log.Information("Changing Discord channel {ChannelName} topic to: {Topic}", _channel.Name, message);
            await _channel.ModifyAsync(x =>
            {
                x.Topic = message;
            });
            Serilog.Log.Debug("Discord channel topic changed successfully");
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to change channel topic. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
        }
    }

    public async Task SendImageAsync(byte[] imageBytes)
    {
        if (_client is null || _channel is null)
            return;

        try
        {
            Serilog.Log.Information("🖼️ Sending Discord image to channel {ChannelName} ({ImageSize} bytes)", _channel.Name, imageBytes.Length);
            var result = await _channel.SendFileAsync(new MemoryStream(imageBytes), "winners");
            Serilog.Log.Information("Discord image sent successfully as message {MessageId}", result.Id);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to send an image. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
        }
    }
}
