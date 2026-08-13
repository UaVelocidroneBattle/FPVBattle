using FluentAssertions;
using Veloci.Logic.Bot.Telegram;

namespace Veloci.Tests;

public class TelegramMarkdownTests
{
    [Theory]
    [InlineData("kim*tendo", "kim\\*tendo")]
    [InlineData("a`b", "a\\`b")]
    [InlineData("a[b]c", "a\\[b\\]c")]
    [InlineData("a~b", "a\\~b")]
    [InlineData("a|b", "a\\|b")]
    [InlineData("a{b}c", "a\\{b\\}c")]
    [InlineData("a>b", "a\\>b")]
    [InlineData("a=b", "a\\=b")]
    [InlineData("a\\b", "a\\\\b")]
    public void Escape_EscapesCharactersWeUseAsMarkup(string text, string expected)
    {
        TelegramMarkdown.Escape(text).Should().Be(expected);
    }

    [Fact]
    public void Escape_LeavesTheRestToTheMessagePass()
    {
        // The two passes cover disjoint sets, so a value is never escaped twice.
        TelegramMarkdown.Escape("fpv.rodriguez").Should().Be("fpv.rodriguez");
    }

    [Fact]
    public void EscapeComposedMessage_KeepsOurOwnMarkupIntact()
    {
        TelegramMarkdown.EscapeComposedMessage("*bold* and `code`")
            .Should().Be("*bold* and `code`");
    }

    [Theory]
    [InlineData("end.", "end\\.")]
    [InlineData("wow!", "wow\\!")]
    [InlineData("a - b", "a \\- b")]
    [InlineData("a + b", "a \\+ b")]
    [InlineData("_Yui_", "\\_Yui\\_")]
    [InlineData("(note)", "\\(note\\)")]
    [InlineData("#tag", "\\#tag")]
    public void EscapeComposedMessage_EscapesCharactersWeNeverUseAsMarkup(string message, string expected)
    {
        TelegramMarkdown.EscapeComposedMessage(message).Should().Be(expected);
    }

    [Fact]
    public void TheTwoPassesCoverEveryReservedCharacterExactlyOnce()
    {
        const string reserved = @"_*[]()~`>#+-=|{}.!\";

        var escaped = TelegramMarkdown.EscapeComposedMessage(TelegramMarkdown.Escape(reserved));

        escaped.Should().Be(string.Concat(reserved.Select(c => $"\\{c}")));
    }

    [Fact]
    public void BothPassesCombined_ProduceValidMarkupForANameContainingAnAsterisk()
    {
        // Regression: the pilot "kim*tendo⁶⁴" left a bold entity unterminated, and Telegram
        // rejected every monthly leaderboard that included them.
        var composed = $"56 - *{TelegramMarkdown.Escape("kim*tendo⁶⁴")}* - 1 балів";

        TelegramMarkdown.EscapeComposedMessage(composed)
            .Should().Be("56 \\- *kim\\*tendo⁶⁴* \\- 1 балів");
    }
}
