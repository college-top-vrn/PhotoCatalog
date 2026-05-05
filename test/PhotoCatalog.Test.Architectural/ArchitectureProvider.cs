using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using Xunit;

namespace PhotoCatalog.Test.Architectural;

public static class ArchitectureProvider
{
    public static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(
                typeof(ApplicationErrors).Assembly,
                typeof(Album).Assembly
            ).Build();

    public static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInNamespaceMatching("PhotoCatalog.Application.*");

    public static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInNamespaceMatching("PhotoCatalog.Domain.*");

    public static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInNamespaceMatching("PhotoCatalog.Infrastructure.*");

    [Fact]
    public static void Domain_ShouldNot_HaveDependencies_OnOtherLayers()
    {
        Classes().That().Are(DomainLayer).Should().NotDependOnAny(ApplicationLayer).Check(Architecture);
    }

    [Fact]
    public static void Application_ShouldNot_HaveDependencies_OnInfrastructure()
    {
        Classes().That().Are(ApplicationLayer).Should().NotDependOnAny(InfrastructureLayer).Check(Architecture);
    }

    [Fact]
    public static void Infrastructure_Should_DependOn_Domain()
    {
        Classes().That().Are(InfrastructureLayer).Should().DependOnAny(DomainLayer).Check(Architecture);
    }

    [Fact]
    public static void DomainClasses_Should_BeInNamespace_That_StartsWith_PhotoCatalogDomain()
    {
        Classes().That().Are(DomainLayer).Should().ResideInNamespaceMatching(@"PhotoCatalog.Domain.*");
    }
}