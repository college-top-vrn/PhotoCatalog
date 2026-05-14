using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Entities;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Тест, проверяющий наличие модификатора sealed для классов из пространства имён PhotoCatalog.Domain.Entities
///     и PhotoCatalog.Domain.ValueObjects.
/// </summary>
public static class EntitySealing
{
    /// <summary>
    ///     Архитектура.
    /// </summary>
    private static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(
                typeof(Album).Assembly)
            .Build();

    /// <summary>
    ///     Классы пространства имени PhotoCatalog.Domain.Entities.
    /// </summary>
    private static readonly IObjectProvider<IType> DomainLayerEntities =
        Types().That().ResideInAssemblyMatching("PhotoCatalog.Domain.Entities.*");

    /// <summary>
    ///     Классы пространства имени PhotoCatalog.Domain.ValueObjects.
    /// </summary>
    private static readonly IObjectProvider<IType> DomainLayerValueObjects =
        Types().That().ResideInAssemblyMatching("PhotoCatalog.Domain.ValueObjects.*");

    /// <summary>
    ///     Тест, проверяющий наличие модификатора sealed у всех классов пространства имени
    ///     PhotoCatalog.Domain.Entities.
    /// </summary>
    [Fact]
    public static void DomainEntities_Should_Be_Sealed()
    {
        Classes()
            .That()
            .Are(DomainLayerEntities)
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .Because("не должны иметь наследников")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    /// <summary>
    ///     Тест, проверяющий наличие модификатора sealed у всех классов пространства имени
    ///     PhotoCatalog.Domain.Entities.
    /// </summary>
    [Fact]
    public static void ValueObjects_Should_Be_Sealed()
    {
        Classes()
            .That()
            .Are(DomainLayerValueObjects)
            .And()
            .AreRecord()
            .Should()
            .BeSealed()
            .Because("не должны иметь наследников")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}