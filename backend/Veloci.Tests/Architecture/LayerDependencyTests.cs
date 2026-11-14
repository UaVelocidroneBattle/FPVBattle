using System.Reflection;
using FluentAssertions;

namespace Veloci.Tests.Architecture;

/// <summary>
/// Pins the dependency direction between the backend projects so a refactoring step cannot quietly reverse it.
/// Each stage of the clean architecture refactor tightens these rules.
/// </summary>
public class LayerDependencyTests
{
    private const string Domain = "Veloci.Domain";
    private const string Data = "Veloci.Data";
    private const string Logic = "Veloci.Logic";
    private const string Web = "Veloci.Web";
    private const string Mcp = "Veloci.Mcp";

    [Theory]
    [InlineData(Domain, Data)]
    [InlineData(Domain, Logic)]
    [InlineData(Domain, Web)]
    [InlineData(Domain, Mcp)]
    [InlineData(Data, Logic)]
    [InlineData(Data, Web)]
    [InlineData(Data, Mcp)]
    [InlineData(Logic, Web)]
    [InlineData(Logic, Mcp)]
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
