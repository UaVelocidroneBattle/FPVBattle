using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Veloci.Logic.Services;

public class ImageService
{
    public async Task<byte[]> CreateWinnerImageAsync(string season, string[] winners)
    {
        const string templateName = "winner-template.png";

        // Season text
        const int seasonTextContainerWidth = 1100;
        const int seasonTextContainerX = 0;
        const int seasonTextContainerY = 260;
        const int seasonFontSize = 24;

        const int winnerFontSize = 24;
        const int winnerTextContainerWidth = 240;

        // Winner 1
        const int winner1TextContainerX = 430;
        const int winner1TextContainerY = 373;

        // Winner 2
        const int winner2TextContainerX = 170;
        const int winner2TextContainerY = 430;

        // Winner 3
        const int winner3TextContainerX = 692;
        const int winner3TextContainerY = 474;

        FontCollection collection = new();
        var family = collection.Add("wwwroot/fonts/tahoma.ttf");

        var templatePath = Path.Combine(Environment.CurrentDirectory, $"wwwroot/images/{templateName}");
        using var template = await Image.LoadAsync(templatePath);

        var seasonFont = family.CreateFont(seasonFontSize, FontStyle.Regular);
        var seasonOptions = new RichTextOptions(seasonFont)
        {
            Origin = new PointF(seasonTextContainerX, seasonTextContainerY),
            WrappingLength = seasonTextContainerWidth,
            TextAlignment = TextAlignment.Center,
        };

        var winnerFont = family.CreateFont(winnerFontSize, FontStyle.Regular);
        var winner1Options = new RichTextOptions(winnerFont)
        {
            Origin = new PointF(winner1TextContainerX, winner1TextContainerY),
            WrappingLength = winnerTextContainerWidth,
            TextAlignment = TextAlignment.Center,
        };

        var winner2Options = new RichTextOptions(winnerFont)
        {
            Origin = new PointF(winner2TextContainerX, winner2TextContainerY),
            WrappingLength = winnerTextContainerWidth,
            TextAlignment = TextAlignment.Center,
        };

        var winner3Options = new RichTextOptions(winnerFont)
        {
            Origin = new PointF(winner3TextContainerX, winner3TextContainerY),
            WrappingLength = winnerTextContainerWidth,
            TextAlignment = TextAlignment.Center,
        };

        var brush = Brushes.Solid(Color.White);
        template.Mutate(x => x
            .DrawText(seasonOptions, season, brush)
            .DrawText(winner1Options, winners[0], brush)
            .DrawText(winner2Options, winners[1], brush)
            .DrawText(winner3Options, winners[2], brush));

        using var stream = new MemoryStream();
        await template.SaveAsync(stream, new PngEncoder
        {
            CompressionLevel = PngCompressionLevel.BestSpeed
        });

        return stream.ToArray();
    }
}
