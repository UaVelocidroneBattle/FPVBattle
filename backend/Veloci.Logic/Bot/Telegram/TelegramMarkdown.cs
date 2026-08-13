using System.Text;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// MarkdownV2 escaping for Telegram messages.
///
/// A composed message mixes markup we write ourselves (<c>*bold*</c>, <c>`code`</c>) with values
/// supplied by pilots, such as pilot, track and quad names. Composers escape the values with
/// <see cref="Escape"/>; the messenger escapes the finished message with
/// <see cref="EscapeComposedMessage"/> just before sending.
///
/// The two cover disjoint character sets — between them every reserved character is escaped
/// exactly once, and no value is escaped twice. Composers never need the second method.
/// </summary>
public static class TelegramMarkdown
{
    /// <summary>
    /// Reserved characters our composers use as markup, plus the escape character itself.
    /// Escaped in supplied values only — escaping them message-wide would strip the formatting
    /// we intend.
    /// </summary>
    private const string MarkupCharacters = @"*`[]~>|{}=\";

    /// <summary>
    /// Reserved characters we never use as markup, so they can be escaped message-wide.
    /// </summary>
    private const string LiteralCharacters = ".!-+_()#";

    /// <summary>
    /// Escapes a value coming from outside the codebase, so that it cannot break the markup of
    /// the message it is placed into. Safe in every position, including inside a code entity,
    /// where Telegram consumes the backslashes just the same.
    /// </summary>
    public static string Escape(string text) => Escape(text, MarkupCharacters);

    /// <summary>
    /// Escapes a finished message, leaving our own markup intact. Called by the messenger on the
    /// way out — composers want <see cref="Escape"/> instead.
    /// </summary>
    public static string EscapeComposedMessage(string message) => Escape(message, LiteralCharacters);

    private static string Escape(string text, string charactersToEscape)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var escaped = new StringBuilder(text.Length);

        foreach (var character in text)
        {
            if (charactersToEscape.Contains(character))
                escaped.Append('\\');

            escaped.Append(character);
        }

        return escaped.ToString();
    }
}
