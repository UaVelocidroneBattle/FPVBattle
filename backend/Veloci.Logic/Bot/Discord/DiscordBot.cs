using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Veloci.Logic.Bot;

public interface IDiscordBot
{
    Task SendMessage(string message);
}

public class DiscordBot : IDiscordBot
{
    private DiscordSocketClient? _client;
    private readonly string? _token;

    public DiscordBot(IConfiguration configuration)
    {
        _token = configuration.GetSection("Discord:BotToken").Value;
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

    private async Task SendMessage(ITextChannel channel, string message)
    {
        if (_client == null) return;

        try
        {
            await channel.SendMessageAsync(message);
        }
        catch (Exception e)
        {
            Serilog.Log.Error(e, "Failed to send message. Guild: {Guild}, Channel: {Channel}", channel.Guild.Name, channel.Name);
        }
    }

    private static Task Log(LogMessage msg)
    {
        Serilog.Log.Verbose(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task Stop()
    {
        if (_client == null) return;

        await _client.StopAsync();
    }

    private static readonly string[] AllowedChannels = ["velocidrone-battle", "лісопед", "track-of-the-day"];

    public async Task SendMessage(string message)
    {
        if (_client == null) return;

        var t = from guild in _client.Guilds
            from channel in guild.Channels.OfType<ITextChannel>()
            where AllowedChannels.Contains(channel.Name)
            select SendMessage(channel, message);

        await Task.WhenAll(t);
    }
}
