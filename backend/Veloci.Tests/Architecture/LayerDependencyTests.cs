using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace Veloci.Tests.Architecture;

/// <summary>
/// Pins the dependency direction between the backend projects so a refactoring step cannot quietly reverse it.
/// Each stage of the clean architecture refactor tightens these rules.
/// </summary>
public class LayerDependencyTests
{
    private const string Data = "Veloci.Data";
    private const string Logic = "Veloci.Logic";
    private const string Web = "Veloci.Web";
    private const string Mcp = "Veloci.Mcp";
    private const string DomainNamespace = "Veloci.Data.Domain";
    private const string EntityFramework = "Microsoft.EntityFrameworkCore";

    /// <summary>
    /// Domain types that still use EF Core. Stage 1 moves their query helpers out of the domain and empties this list;
    /// until then a new entry fails the test, and so does an entry that no longer needs to be here.
    /// </summary>
    private static readonly string[] DomainTypesKnownToUseEntityFramework =
    [
        "Veloci.Data.Domain.PilotExtensions",
    ];

    [Theory]
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
    public void Domain_does_not_depend_on_persistence()
    {
        var result = DomainTypes()
            .ShouldNot()
            .HaveDependencyOnAny($"{Data}.Repositories", $"{Data}.ApplicationDbContext")
            .GetResult();

        result.FailingTypeNames.Should().BeNullOrEmpty("domain types must not know how they are stored");
    }

    [Fact]
    public void Domain_does_not_take_new_dependencies_on_entity_framework()
    {
        var result = DomainTypes()
            .ShouldNot()
            .HaveDependencyOn(EntityFramework)
            .GetResult();

        (result.FailingTypeNames ?? []).Should().BeEquivalentTo(DomainTypesKnownToUseEntityFramework,
            "domain types must not use EF Core; move query helpers that need it out of the domain");
    }

    private static PredicateList DomainTypes() =>
        Types.InAssembly(Assembly.Load(Data)).That().ResideInNamespace(DomainNamespace);
}
