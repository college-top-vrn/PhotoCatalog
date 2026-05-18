using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Interfaces.Repositories;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Архитектурные тесты, которые проверяют
///     правильность расположения интерфейсов.
/// </summary>
public class ContractTests
{
    // TODO: Добавить в загрузку необходимые классы из Infrastructure, когда они появятся
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(IPhotoRepository).Assembly,
            typeof(IAlbumCommandRepository).Assembly,
            typeof(IFolderRepository).Assembly,
            typeof(ITagRepository).Assembly
        ).Build();

    /// <summary>
    ///     Архитектурный тест, проверяющий, что
    ///     все интерфейсы, имя которых заканчивается
    ///     на Repository (например, IPhotoRepository),
    ///     должны находиться исключительно в слое Domain.
    /// </summary>
    [Fact]
    public void RepositoryInterfaces_Should_ResideInDomain()
    {
        Interfaces()
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain($|\..*)")
            .Because(
                "Все классы в слое Infrastructure, реализующие логику работы с данными или файлами, должны реализовывать интерфейсы из слоя Domain.")
            .Check(Architecture);
    }

    /// <summary>
    ///     Архитектурный тест, проверяющий, что
    ///     все классы в слое Infrastructure,
    ///     реализующие логику работы с данными или файлами,
    ///     должны реализовывать интерфейсы из слоя Domain.
    /// </summary>
    [Fact]
    public void InfrastructureServices_Should_ImplementDomainInterfaces()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Infrastructure($|\..*)")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementAnyInterfacesThat()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain($|\..*)")
            .Because(
                "Все классы в слое Infrastructure, реализующие логику работы с данными или файлами, должны реализовывать интерфейсы из слоя Domain.")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    /// <summary>
    ///     Архитектурный тест, проверяющий, что
    ///     реализации репозиториев
    ///     (классы вроде SqlitePhotoRepository)
    ///     не могут находиться в Domain или Application.
    /// </summary>
    [Fact]
    public void RepositoryImplementations_Should_ResideInInfrastructure()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementAnyInterfacesThat()
            .HaveNameEndingWith("Repository")
            .AndShould()
            .NotResideInNamespaceMatching(@"^PhotoCatalog\.Domain($|\..*)")
            .AndShould()
            .NotResideInNamespaceMatching(@"^PhotoCatalog\.Application($|\..*)")
            .Because(
                "Реализации репозиториев (классы вроде SqlitePhotoRepository) не могут находиться в Domain или Application.")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}