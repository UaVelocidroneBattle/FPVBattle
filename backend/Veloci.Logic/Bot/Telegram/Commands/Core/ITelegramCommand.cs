namespace Veloci.Logic.Bot.Telegram.Commands.Core;

public interface ITelegramCommand
{
    /// <summary>
    /// Synonyms for the command
    /// </summary>
    public string[] Keywords { get; }

    public string Description { get; }

    /// <summary>
    /// Executes the command with cup context
    /// </summary>
    /// <param name="context">Command context including cup ID, chat ID, and parameters</param>
    /// <returns>Response message to send back to the user</returns>
    public Task<string> ExecuteAsync(TelegramCommandContext context);

    public bool RemoveMessageAfterDelay { get; }

    public bool Public { get; }
}
