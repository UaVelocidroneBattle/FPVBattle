namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public interface ITelegramCommand
{
    /// <summary>
    /// Synonyms for the command
    /// </summary>
    public string[] Keywords { get; }
    public string Description { get; }
    public Task<string> ExecuteAsync(string[]? parameters);
    public bool RemoveMessageAfterDelay { get; }
    public bool Public { get; }
}
