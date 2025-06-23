using FluentAssertions;
using Veloci.Data.Domain;

namespace Veloci.Tests;

public class CompetitionVariablesTests
{
    [Fact]
    public void AddOrUpdateVariable_ShouldAddNewVariable_WhenVariableDoesNotExist()
    {
        // Arrange
        var competition = new Competition
        {
            Variables = new List<CompetitionVariable>()
        };

        var variableName = "TestVariable";
        var variableValue = "TestValue";

        // Act
        competition.AddOrUpdateVariable(variableName, variableValue);

        // Assert
        var variable = competition.GetVariable(variableName);
        variable.Should().NotBeNull();
        variable.Name.Should().Be(variableName);
        variable.StringValue.Should().Be(variableValue);
    }

    [Fact]
    public void AddOrUpdateVariable_ShouldUpdateExistingVariable_WhenVariableExists()
    {
        // Arrange
        var competition = new Competition
        {
            Variables = new List<CompetitionVariable>
            {
                new CompetitionVariable { Name = "TestVariable", StringValue = "OldValue" }
            }
        };

        var variableName = "TestVariable";
        var variableValue = "NewValue";

        // Act
        competition.AddOrUpdateVariable(variableName, variableValue);

        // Assert
        var variable = competition.GetVariable(variableName);
        variable.Should().NotBeNull();
        variable.Name.Should().Be(variableName);
        variable.StringValue.Should().Be(variableValue);
    }

    [Fact]
    public void GetVariable_ShouldReturnNull_WhenVariableDoesNotExist()
    {
        // Arrange
        var competition = new Competition
        {
            Variables = new List<CompetitionVariable>()
        };

        var variableName = "NonExistentVariable";

        // Act
        var variable = competition.GetVariable(variableName);

        // Assert
        variable.Should().BeNull();
    }

    [Fact]
    public void GetVariable_ShouldReturnVariable_WhenVariableExists()
    {
        // Arrange
        var competition = new Competition
        {
            Variables = new List<CompetitionVariable>
            {
                new CompetitionVariable { Name = "ExistingVariable", StringValue = "Value" }
            }
        };

        var variableName = "ExistingVariable";

        // Act
        var variable = competition.GetVariable(variableName);

        // Assert
        variable.Should().NotBeNull();
        variable.Name.Should().Be(variableName);
    }

    [Fact]
    public void UpdateValue_ShouldUpdateStringValue_WhenStringValueIsProvided()
    {
        // Arrange
        var variable = new CompetitionVariable { Name = "TestVariable" };
        var newValue = "NewStringValue";

        // Act
        variable.UpdateValue(newValue);

        // Assert
        variable.StringValue.Should().Be(newValue);
    }

    [Fact]
    public void UpdateValue_ShouldUpdateLongValue_WhenIntValueIsProvided()
    {
        // Arrange
        var variable = new CompetitionVariable { Name = "TestVariable" };
        int newValue = 1234;

        // Act
        variable.UpdateValue(newValue);

        // Assert
        variable.IntValue.Should().Be(newValue);
    }

    [Fact]
    public void UpdateValue_ShouldUpdateLongValue_WhenULongValueIsProvided()
    {
        // Arrange
        var variable = new CompetitionVariable { Name = "TestVariable" };
        ulong newValue = 1234567890;

        // Act
        variable.UpdateValue(newValue);

        // Assert
        variable.ULongValue.Should().Be(newValue);
    }

    [Fact]
    public void UpdateValue_ShouldUpdateDoubleValue_WhenDoubleValueIsProvided()
    {
        // Arrange
        var variable = new CompetitionVariable { Name = "TestVariable" };
        double newValue = 1234.56;

        // Act
        variable.UpdateValue(newValue);

        // Assert
        variable.DoubleValue.Should().Be(newValue);
    }
}
