using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Interfaces.Repositories;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Содержит архитектурные тесты для проверки соблюдения принципа инверсии зависимостей
///     и изоляции контрактов.
/// </summary>
public class InterfaceLocalizationTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(IPhotoRepository).Assembly
        ).Build();

    /// <summary>
    ///     Проверяет, что все интерфейсы находятся только в слое Domain в папке Interfaces.
    /// </summary>
    [Fact]
    public void AllInterfaces_Should_ResideIn_DomainInterfaces_Namespace()
    {
        Interfaces()
            .That()
            .AreNot(typeof(System.IDisposable))
            .Should()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain\.Interfaces(\..*)?$")
            .Because("Все интерфейсы должны быть в PhotoCatalog.Domain.Interfaces с подпапками")
            .Check(Architecture);
    }
}