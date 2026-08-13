using System.Text;

namespace Veloci.Logic.Bot.Telegram;

/// <summary>
/// MarkdownV2 escaping for Telegram messages.
///
/// A composed message mixes markup we write ourselves (<c>*bold*</c>, <c>`code`</c>) with values
/// supplied by pilots, such as pilot, track and quad names. The two need opposite treatment, so
/// escaping happens in two passes over disjoint character sets:
///
/// <list type="bullet">
/// <item><see cref="EscapeUserText"/> and <see cref="EscapeCodeText"/> are applied by the composers
/// to every user-supplied value, and escape the reserved characters we use as markup. Without this,
/// a name like <c>kim*tendo</c> leaves a bold entity unterminated and Telegram rejects the whole
/// message.</item>
/// <item><see cref="EscapeMessage"/> is applied by the messenger to the finished message, and
/// escapes the remaining reserved characters, which never carry meaning for us.</item>
/// </list>
///
/// Because the sets are disjoint, a value is never escaped twice.
/// </summary>
public static class TelegramMarkdown
{
    /// <summary>
    /// Reserved characters our composers use as markup, plus the escape character itself.
    /// Escaped in user-supplied values only — escaping them message-wide would strip the
    /// formatting we intend.
    /// </summary>
    private const string MarkupCharacters = @"*`[]~>|{}=\";

    /// <summary>
    /// Reserved characters we never use as markup, so they can be escaped message-wide.
    /// </summary>
    private const string LiteralCharacters = ".!-+_()#";

    /// <summary>
    /// The only characters that carry meaning inside a code entity.
    /// </summary>
    private const string CodeCharacters = @"`\";

    /// <summary>
    /// Escapes a finished message, leaving our own markup intact.
    /// </summary>
    public static string EscapeMessage(string message) => Escape(message, LiteralCharacters);

    /// <summary>
    /// Escapes a user-supplied value interpolated into regular text or markup.
    /// </summary>
    public static string EscapeUserText(string text) => Escape(text, MarkupCharacters);

    /// <summary>
    /// Escapes text interpolated inside a <c>`code`</c> entity, where our markup characters are
    /// already literal and escaping them would show stray backslashes.
    /// </summary>
    public static string EscapeCodeText(string text) => Escape(text, CodeCharacters);

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
