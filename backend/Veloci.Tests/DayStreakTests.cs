using FluentAssertions;
using Veloci.Data.Domain;

namespace Veloci.Tests;

public class DayStreakTests
{
    [Fact]
    public void increase_day_streak_should_increase_streak()
    {
        // Arrange
        var pilot = new Pilot("TestPilot");
        var today = DateTime.Now;

        // Act
        pilot.IncreaseDayStreak(today);

        // Assert
        pilot.DayStreak.Should().Be(1);
        pilot.LastRaceDate.Should().Be(today);
    }

    [Fact]
    public void increase_streak_from_1_freezes_should_be_0()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 1
        };

        var today = DateTime.Now;
        pilot.IncreaseDayStreak(today);

        // Assert
        pilot.DayStreak.Should().Be(2);
        pilot.DayStreakFreezes.Should().Be(0);
    }

    [Fact]
    public void increase_streak_from_29_freezes_should_be_1()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 29
        };

        var today = DateTime.Now;
        pilot.IncreaseDayStreak(today);

        // Assert
        pilot.DayStreak.Should().Be(30);
        pilot.DayStreakFreezes.Should().Be(1);
    }

    [Fact]
    public void increase_streak_from_59_freezes_should_be_1()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 59
        };

        var today = DateTime.Now;
        pilot.IncreaseDayStreak(today);

        // Assert
        pilot.DayStreak.Should().Be(60);
        pilot.DayStreakFreezes.Should().Be(1);
    }

    [Fact]
    public void reset_streak_when_has_2_freezes()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 50,
            DayStreakFreezes = 2
        };

        pilot.ResetDayStreak();

        // Assert
        pilot.DayStreak.Should().Be(50);
        pilot.DayStreakFreezes.Should().Be(1);
    }

    [Fact]
    public void reset_streak_when_has_0_freezes()
    {
        var pilot = new Pilot("TestPilot")
        {
            DayStreak = 50,
            DayStreakFreezes = 0
        };

        pilot.ResetDayStreak();

        // Assert
        pilot.DayStreak.Should().Be(0);
        pilot.DayStreakFreezes.Should().Be(0);
    }
}
