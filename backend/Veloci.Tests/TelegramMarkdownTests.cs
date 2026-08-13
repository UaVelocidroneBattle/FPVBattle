using FluentAssertions;
using Veloci.Logic.Bot.Telegram;

namespace Veloci.Tests;

public class TelegramMarkdownTests
{
    [Fact]
    public void EscapeMessage_KeepsOurOwnMarkupIntact()
    {
        TelegramMarkdown.EscapeMessage("*bold* and `code`")
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
    public void EscapeMessage_EscapesCharactersWeNeverUseAsMarkup(string message, string expected)
    {
        TelegramMarkdown.EscapeMessage(message).Should().Be(expected);
    }

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
    public void EscapeUserText_EscapesCharactersWeUseAsMarkup(string text, string expected)
    {
        TelegramMarkdown.EscapeUserText(text).Should().Be(expected);
    }

    [Fact]
    public void EscapeUserText_LeavesEscapingOfTheRestToTheMessagePass()
    {
        // The two passes cover disjoint character sets, so a value is never escaped twice.
        TelegramMarkdown.EscapeUserText("fpv.rodriguez").Should().Be("fpv.rodriguez");
    }

    [Fact]
    public void EscapeCodeText_OnlyEscapesWhatBreaksACodeEntity()
    {
        TelegramMarkdown.EscapeCodeText("a*b`c\\d").Should().Be("a*b\\`c\\\\d");
    }

    [Fact]
    public void BothPassesCombined_ProduceValidMarkupForANameContainingAnAsterisk()
    {
        // Regression: the pilot "kim*tendo⁶⁴" left a bold entity unterminated, and Telegram
        // rejected every monthly leaderboard that included them.
        var composed = $"56 - *{TelegramMarkdown.EscapeUserText("kim*tendo⁶⁴")}* - 1 балів";

        TelegramMarkdown.EscapeMessage(composed)
            .Should().Be("56 \\- *kim\\*tendo⁶⁴* \\- 1 балів");
    }
}
