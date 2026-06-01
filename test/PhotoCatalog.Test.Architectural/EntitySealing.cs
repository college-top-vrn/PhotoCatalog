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
    ///     Архитектура доменного слоя.
    /// </summary>
    private static readonly Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(typeof(Album).Assembly)
            .Build();

    /// <summary>
    ///     Тест, проверяющий наличие модификатора sealed у всех классов пространства имени
    ///     PhotoCatalog.Domain.Entities.
    /// </summary>
    [Fact]
    public static void DomainEntitiesShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInAssembly(typeof(Album).Assembly)
            .And()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain\.Entities(\..*)?$")
            .And()
            .AreNotAbstract()
            .And()
            .AreNotSealed()
            .Should()
            .NotExist()
            .Because("не должны иметь наследников")
            .Check(Architecture);
    }

    /// <summary>
    ///     Тест, проверяющий наличие модификатора sealed у всех record-классов пространства имени
    ///     PhotoCatalog.Domain.ValueObjects.
    /// </summary>
    [Fact]
    public static void ValueObjectsShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInAssembly(typeof(Album).Assembly)
            .And()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain\.ValueObjects(\..*)?$")
            .And()
            .AreRecord()
            .And()
            .AreNotSealed()
            .Should()
            .NotExist()
            .Because("не должны иметь наследников")
            .Check(Architecture);
    }
}