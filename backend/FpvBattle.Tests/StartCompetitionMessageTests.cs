using FluentAssertions;
using FpvBattle.Application.Bot.Discord;
using FpvBattle.Application.Bot.Telegram;
using FpvBattle.Domain;

namespace FpvBattle.Tests;

public class StartCompetitionMessageTests
{
    private readonly Track _track = new() { TrackId = 101, Name = "Hangar Race", Scene = new Scene { SceneId = 3, Name = "Hangar" } };

    [Fact]
    public void Telegram_ShouldNameThePatron_WhenAPatronPickedTheTrack()
    {
        var message = new TelegramMessageComposer().StartCompetition(_track, quadOfTheDay: null, pickedByPatron: "kim*tendo");

        message.Should().Contain("Picked by *kim\\*tendo*");
    }

    [Fact]
    public void Discord_ShouldNameThePatron_WhenAPatronPickedTheTrack()
    {
        var message = new DiscordMessageComposer().StartCompetition(_track, quadOfTheDay: null, pickedByPatron: "kimtendo");

        message.Should().Contain("Picked by **kimtendo**");
    }

    [Fact]
    public void StartMessages_ShouldNotNameAPatron_WhenTheTrackWasNotPickedByOne()
    {
        new TelegramMessageComposer().StartCompetition(_track, null, null).Should().NotContain("Picked by");
        new DiscordMessageComposer().StartCompetition(_track, null, null).Should().NotContain("Picked by");
    }
}
