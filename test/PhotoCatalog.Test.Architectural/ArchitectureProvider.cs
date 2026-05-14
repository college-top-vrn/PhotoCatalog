using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Application.Errors;
using PhotoCatalog.Domain.Entities;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Архитектурный тест, проверяющий зависимости своими слоями.
/// </summary>
public static class ArchitectureProvider
{
    /// <summary>
    ///     Архитектура.
    /// </summary>
    public static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(
                typeof(ApplicationErrors).Assembly,
                typeof(Album).Assembly
            ).Build();

    /// <summary>
    ///     Прикладной выбора.
    /// </summary>
    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types()
            .That()
            .ResideInNamespaceMatching("PhotoCatalog.Application.*");

    /// <summary>
    ///     Слой домена.
    /// </summary>
    private static readonly IObjectProvider<IType> DomainLayer =
        Types()
            .That()
            .ResideInNamespaceMatching("PhotoCatalog.Domain.*");

    /// <summary>
    ///     Слой инфраструктуры.
    /// </summary>
    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types()
            .That()
            .ResideInNamespaceMatching("PhotoCatalog.Infrastructure.*");

    /// <summary>
    ///     Тест, проверяющий на независимость доменного слоя от других слоёв. 
    /// </summary>
    [Fact]
    public static void Domain_ShouldNot_HaveDependencies_OnOtherLayers()
    {
        Classes()
            .That()
            .Are(DomainLayer)
            .Should()
            .NotDependOnAny(ApplicationLayer)
            .AndShould()
            .NotDependOnAny(InfrastructureLayer)
            .AndShould()
            .NotDependOnAny(ApplicationLayer)
            .Check(Architecture);
    }

    /// <summary>
    ///     Тест, проверяющий на независимость прикладного слоя от слоя инфраструктуры.
    /// </summary>
    [Fact]
    public static void Application_ShouldNot_HaveDependencies_OnInfrastructure()
    {
        Classes()
            .That()
            .Are(ApplicationLayer)
            .Should()
            .NotDependOnAny(InfrastructureLayer)
            .Check(Architecture);
    }

    /// <summary>
    ///     Тест, проверяющий зависимость слоя инфраструктуры на доменный слой.
    /// </summary>
    [Fact]
    public static void Infrastructure_Should_DependOn_Domain()
    {
        Classes()
            .That()
            .Are(InfrastructureLayer)
            .Should()
            .DependOnAny(DomainLayer)
            .Because("Отсутствуют классы в инфраструктуре")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    /// <summary>
    ///     Тест, который проверяет, что пространства имён всех классов в PhotoCatalog.Domain начинаются на PhotoCatalog.Domain
    /// </summary>
    [Fact]
    public static void DomainClasses_Should_BeInNamespace_That_StartsWith_PhotoCatalogDomain()
    {
        Classes()
            .That()
            .Are(DomainLayer)
            .Should()
            .ResideInNamespaceMatching("PhotoCatalog.Domain.*");
    }
}