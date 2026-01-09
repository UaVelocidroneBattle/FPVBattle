using FluentAssertions;
using Veloci.Logic.Services;

namespace Veloci.Tests;

public class PointsCalculatorTests
{
    private readonly PointsCalculator _calculator;

    public PointsCalculatorTests()
    {
        _calculator = new PointsCalculator();
    }

    [Fact]
    public void PointsByPosition_ShouldReturnMaxPoints_ForPosition1()
    {
        // Act
        var points = _calculator.PointsByPosition(1);

        // Assert
        points.Should().Be(100);
    }

    [Fact]
    public void PointsByPosition_Return50Positions()
    {
        // Act
        var positionsAndPoints = new Dictionary<int, int>();

        for (var i = 0; i < 50; i++)
        {
            var position = i + 1;
            var points = _calculator.PointsByPosition(position);
            positionsAndPoints.Add(position, points);
        }

        // Assert
        positionsAndPoints.Count.Should().Be(50);
    }

    [Fact]
    public void PointsByPosition_ShouldReturnMinPoints_ForVeryLargePosition()
    {
        // Act
        var points = _calculator.PointsByPosition(1000);

        // Assert
        points.Should().Be(1);
    }

    [Fact]
    public void PointsByPosition_ShouldNeverReturnLessThanMinPoints()
    {
        // Act & Assert
        for (int position = 1; position <= 100; position++)
        {
            var points = _calculator.PointsByPosition(position);
            points.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public void PointsByPosition_ShouldNeverReturnMoreThanMaxPoints()
    {
        // Act & Assert
        for (int position = 1; position <= 100; position++)
        {
            var points = _calculator.PointsByPosition(position);
            points.Should().BeLessThanOrEqualTo(100);
        }
    }

    [Fact]
    public void PointsByPosition_ShouldReturnDecreasingPoints_AsPositionIncreases()
    {
        // Arrange
        var previousPoints = int.MaxValue;

        // Act & Assert
        for (int position = 1; position < 50; position++)
        {
            var points = _calculator.PointsByPosition(position);
            points.Should().BeLessThanOrEqualTo(previousPoints);
            previousPoints = points;
        }
    }

    [Fact]
    public void PointsByPosition_ShouldThrowArgumentOutOfRangeException_ForZeroPosition()
    {
        // Act
        Action act = () => _calculator.PointsByPosition(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PointsByPosition_ShouldThrowArgumentOutOfRangeException_ForNegativePosition()
    {
        // Act
        Action act = () => _calculator.PointsByPosition(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
