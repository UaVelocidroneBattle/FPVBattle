using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Veloci.Logic.Bot.Discord;

public interface IDiscordBot
{
    Task SendMessageAsync(string message);
    Task ChangeChannelTopicAsync(string message);
}

public class DiscordBot : IDiscordBot
{
    private DiscordSocketClient? _client;
    private readonly string? _token;
    private readonly string[] _allowedChannels;

    public DiscordBot(IConfiguration configuration)
    {
        _token = configuration.GetSection("Discord:BotToken").Value;
        _allowedChannels = configuration
            .GetSection("Discord:Channels")
            .GetChildren()
            .ToArray()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrEmpty(x))
            .Cast<string>()
            .ToArray();

        Serilog.Log.Debug("Allowed discord channels: {@channels}", _allowedChannels);
    }

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

    public async Task SendMessageAsync(string message)
    {
        if (_client is null)
            return;

        var t = from guild in _client.Guilds
            from channel in guild.Channels.OfType<ITextChannel>()
            where _allowedChannels.Contains(channel.Name)
            select SendMessage(channel, message);

        await Task.WhenAll(t);
    }



    private async Task SendMessage(ITextChannel channel, string message)
    {
        if (_client is null)
            return;

        try
        {
            await channel.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to send message. Guild: {Guild}, Channel: {Channel}", channel.Guild.Name, channel.Name);
        }
    }

    public async Task ChangeChannelTopicAsync(string message)
    {
        if (_client is null)
            return;

        var t = from guild in _client.Guilds
            from channel in guild.Channels.OfType<ITextChannel>()
            where _allowedChannels.Contains(channel.Name)
            select ChangeChannelTopicAsync(channel, message);

        await Task.WhenAll(t);
    }

    private async Task ChangeChannelTopicAsync(ITextChannel channel, string message)
    {
        if (_client is null)
            return;

        try
        {
            await channel.ModifyAsync(x =>
            {
                x.Topic = message;
            });
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to change channel topic. Guild: {Guild}, Channel: {Channel}", channel.Guild.Name, channel.Name);
        }
    }
}
