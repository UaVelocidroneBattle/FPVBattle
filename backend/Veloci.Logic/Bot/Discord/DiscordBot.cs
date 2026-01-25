namespace Veloci.Logic.Bot.Discord;

public interface IDiscordBot
{
    Task<ulong?> SendMessageAsync(string message);
    Task EditMessageAsync(ulong messageId, string message);
    Task SendMessageInThreadAsync(ulong messageId, string threadName, string message);
    Task ArchiveThreadAsync(string threadName);
    Task ChangeChannelTopicAsync(string message);
    Task SendImageAsync(byte[] imageBytes, string imageName);
}
