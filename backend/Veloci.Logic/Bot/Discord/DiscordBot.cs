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
            var result = await _channel.SendMessageAsync(message);
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
            await _channel.ModifyMessageAsync(messageId, x =>
            {
                x.Content = message;
            });
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
                thread = await _channel.CreateThreadAsync(threadName, ThreadType.PublicThread, ThreadArchiveDuration.OneDay, messageToReply);
            }

            await thread.SendMessageAsync(message);

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
            await thread.ModifyAsync(x => x.Archived = true);
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
            await _channel.ModifyAsync(x =>
            {
                x.Topic = message;
            });
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to change channel topic. Guild: {Guild}, Channel: {Channel}", _channel.Guild.Name, _channel.Name);
        }
    }
}
