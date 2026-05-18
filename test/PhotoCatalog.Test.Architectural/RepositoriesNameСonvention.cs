using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Interfaces.Repositories;
using PhotoCatalog.Infrastructure.Repositories;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Содержит архитектурные тесты для проверки соблюдения правил проектирования,
///     правильного использования модификаторов доступа и защиты инкапсуляции.
/// </summary>
public class RepositoriesNameСonvention
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(IPhotoRepository).Assembly,
            typeof(SqliteAlbumRepository).Assembly,
            typeof(SqliteFolderRepository).Assembly
        ).Build();

    [Fact]
    public void RepositoryInterfacesShouldHaveRepositorySuffix()
    {
        Interfaces()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog.Domain\.Interfaces\.Repositories(\..*)?$")
            .Should().HaveNameMatching(".*Repository$")
            .Because("Все интерфейсы репозиториев должны иметь суффикс 'Repository' для идентификации их роли")
            .Check(Architecture);
    }

    [Fact]
    public void RepositoryImplementationsShouldHaveRepositorySuffix()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog.Infrastructure\.Repositories(\..*)?$")
            .And().AreNotAbstract()
            .Should().HaveNameMatching(".*Repository$")
            .Because("Реализации репозиториев должны иметь суффикс 'Repository'")
            .Check(Architecture);
    }
}