using System.ComponentModel;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Veloci.Logic.Features.Leagues;

namespace Veloci.Tests;

public class LookBackPeriodTests
{
    [Theory]
    [InlineData("1 month", 1, LookBackUnit.Month)]
    [InlineData("2 months", 2, LookBackUnit.Month)]
    [InlineData("30 days", 30, LookBackUnit.Day)]
    [InlineData("1 day", 1, LookBackUnit.Day)]
    [InlineData("2 weeks", 2, LookBackUnit.Week)]
    [InlineData("  3   WEEKS  ", 3, LookBackUnit.Week)]
    public void Parse_ReadsCountAndUnit(string value, int count, LookBackUnit unit)
    {
        LookBackPeriod.Parse(value).Should().Be(new LookBackPeriod(count, unit));
    }

    [Theory]
    [InlineData("")]
    [InlineData("month")]
    [InlineData("1")]
    [InlineData("0 months")]
    [InlineData("-1 months")]
    [InlineData("1 fortnight")]
    [InlineData("1.5 months")]
    public void Parse_RejectsAnythingElse(string value)
    {
        var parse = () => LookBackPeriod.Parse(value);

        parse.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("2026-09-01", "2026-08-01")] // 31 day month
    [InlineData("2026-05-01", "2026-04-01")] // 30 day month
    [InlineData("2026-03-01", "2026-02-01")] // 28 day month
    [InlineData("2024-03-01", "2024-02-01")] // 29 day leap month
    public void OneMonth_EndingOnTheFirst_StartsOnTheFirstOfTheMonthBefore(string end, string expectedStart)
    {
        var period = new LookBackPeriod(1, LookBackUnit.Month);

        period.StartOfWindowEndingAt(DateTime.Parse(end))
            .Should().Be(DateTime.Parse(expectedStart));
    }

    [Fact]
    public void Days_AndWeeks_CountBackwardsFromTheEnd()
    {
        var end = new DateTime(2026, 8, 23);

        new LookBackPeriod(30, LookBackUnit.Day).StartOfWindowEndingAt(end).Should().Be(new DateTime(2026, 7, 24));
        new LookBackPeriod(2, LookBackUnit.Week).StartOfWindowEndingAt(end).Should().Be(new DateTime(2026, 8, 9));
    }

    [Theory]
    [InlineData(1, LookBackUnit.Month, "1 month")]
    [InlineData(2, LookBackUnit.Week, "2 weeks")]
    [InlineData(30, LookBackUnit.Day, "30 days")]
    public void ToString_ReadsBackTheWayItWasWritten(int count, LookBackUnit unit, string expected)
    {
        new LookBackPeriod(count, unit).ToString().Should().Be(expected);
    }

    [Fact]
    public void TypeConverter_RoundTripsThroughItsOwnText()
    {
        var converter = TypeDescriptor.GetConverter(typeof(LookBackPeriod));
        var period = new LookBackPeriod(2, LookBackUnit.Week);

        converter.CanConvertFrom(typeof(string)).Should().BeTrue();
        converter.ConvertFromInvariantString(period.ToString()).Should().Be(period);
    }

    [Fact]
    public void Binds_FromConfigurationAsAPlainString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PaceRating:MinDaysForRelevance"] = "15",
                ["PaceRating:LookBackPeriod"] = "1 month"
            })
            .Build();

        var settings = configuration.GetSection(PaceRatingSettings.SectionName).Get<PaceRatingSettings>()!;

        settings.MinDaysForRelevance.Should().Be(15);
        settings.LookBackPeriod.Should().Be(new LookBackPeriod(1, LookBackUnit.Month));
    }

    [Fact]
    public void Binding_LeavesTheDefaultAloneWhenTheKeyIsAbsent()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection([]).Build();

        var settings = configuration.GetSection(PaceRatingSettings.SectionName).Get<PaceRatingSettings>()
                       ?? new PaceRatingSettings();

        settings.LookBackPeriod.Should().Be(new LookBackPeriod(1, LookBackUnit.Month));
    }
}
