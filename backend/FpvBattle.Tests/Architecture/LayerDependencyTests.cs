using System.Reflection;
using FluentAssertions;

namespace FpvBattle.Tests.Architecture;

/// <summary>
/// Pins the dependency direction between the backend projects so a refactoring step cannot quietly reverse it.
/// Each stage of the clean architecture refactor tightens these rules.
/// </summary>
public class LayerDependencyTests
{
    private const string Domain = "FpvBattle.Domain";
    private const string Infrastructure = "FpvBattle.Infrastructure";
    private const string Logic = "FpvBattle.Logic";
    private const string Web = "FpvBattle.Web";
    private const string Mcp = "FpvBattle.Mcp";

    [Theory]
    [InlineData(Domain, Infrastructure)]
    [InlineData(Domain, Logic)]
    [InlineData(Domain, Web)]
    [InlineData(Domain, Mcp)]
    [InlineData(Infrastructure, Web)]
    [InlineData(Infrastructure, Mcp)]
    [InlineData(Logic, Infrastructure)]
    [InlineData(Logic, Web)]
    [InlineData(Logic, Mcp)]
    [InlineData(Mcp, Infrastructure)]
    [InlineData(Mcp, Web)]
    public void Project_does_not_reference_outer_project(string project, string outerProject)
    {
        var references = Assembly.Load(project).GetReferencedAssemblies().Select(reference => reference.Name);

        references.Should().NotContain(outerProject, $"{project} must not reference {outerProject}");
    }

    [Fact]
    public void Domain_does_not_reference_entity_framework()
    {
        var references = Assembly.Load(Domain).GetReferencedAssemblies().Select(reference => reference.Name);

        references.Should().NotContain(name => name!.StartsWith("Microsoft.EntityFrameworkCore"),
            "domain types must not know how they are stored");
    }
}
