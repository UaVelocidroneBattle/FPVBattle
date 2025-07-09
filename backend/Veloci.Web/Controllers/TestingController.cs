using Microsoft.AspNetCore.Mvc;
using Veloci.Logic.Bot.Discord;
using Veloci.Logic.Services;

namespace Veloci.Web.Controllers;

[ApiController]
[Route("/test/[action]")]
public class TestingController
{
    private readonly ImageService _imageService;
    private readonly IDiscordBot _discordBot;

    public TestingController(ImageService imageService, IDiscordBot discordBot)
    {
        _imageService = imageService;
        _discordBot = discordBot;
    }

    [HttpGet]
    public async Task ImageTest()
    {
        var seasonName = "June 2025";
        var winners = new[] { "winner 1", "winner 2", "winner 3" };

        var image = await _imageService.CreateWinnerImageAsync(seasonName, winners);
        await _discordBot.SendImageAsync(image);
    }
}
