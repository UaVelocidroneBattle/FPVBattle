namespace Veloci.Logic.Bot.Telegram.Commands.Core;

/// <summary>
/// Context information for executing a Telegram command
/// </summary>
/// <remarks>
/// Provides cup context resolution for commands, allowing them to operate
/// within the appropriate competition cup (5-inch, whoop, etc.).
/// </remarks>
public class TelegramCommandContext
{
    /// <summary>
    /// Telegram chat ID where the command was received
    /// </summary>
    public required string ChatId { get; init; }

    /// <summary>
    /// Cup ID resolved from the chat ID, or null if chat is not bound to any cup
    /// </summary>
    /// <remarks>
    /// When null, commands should either return a message indicating the chat is unbound,
    /// or operate in a global context (e.g., help commands).
    /// </remarks>
    public required string? CupId { get; init; }

    /// <summary>
    /// Command parameters (arguments following the command)
    /// </summary>
    public string[]? Parameters { get; init; }
}
