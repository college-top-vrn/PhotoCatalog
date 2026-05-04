using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Interfaces.Repositories;

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
            typeof(IPhotoRepository).Assembly
        ).Build();

    [Fact]
    public void RepositoryInterfaces_Should_HaveRepositorySuffix()
    {
        Interfaces()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog.Domain\.Interfaces\.Repositories(\..*)?$")
            .Should().HaveNameMatching(".*Repository$")
            .Because("Все интерфейсы репозиториев должны иметь суффикс 'Repository' для идентификации их роли")
            .Check(Architecture);
    }

    [Fact]
    public void RepositoryImplementations_Should_HaveRepositorySuffix()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog.Domain\.Entities\.Repositories(\..*)?$")
            .And().AreNotAbstract()
            .Should().HaveNameMatching(".*Repository$")
            .Because("Реализации репозиториев должны иметь суффикс 'Repository'")
            .Check(Architecture);
    }
}