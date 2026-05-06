using ArchUnitNET.Domain;
using ArchUnitNET.Fluent.Conditions;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Entities;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Архитектурные тесты, которые
///     защищают инкапсуляцию доменных сущностей.
/// </summary>
public class EncapsulationTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Album).Assembly,
            typeof(Folder).Assembly,
            typeof(Photo).Assembly,
            typeof(Tag).Assembly
        ).Build();

    /// <summary>
    ///     Архитектурный тест, проверяющий, что
    ///     все классы-сущности в папке/неймспейсе Entities
    ///     не должны иметь публичных сеттеров (public set;).
    ///     Разрешены только private set; или init;.
    /// </summary>
    [Fact]
    public void DomainEntities_ShouldNot_HavePublicSetters()
    {
        GivenClassesConjunction? entities = Classes()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain\.Entities($|\..*)")
            .And()
            .AreNotAbstract();

        PropertyMembers()
            .That()
            .ArePublic()
            .And()
            .AreDeclaredIn(entities)
            .Should()
            .NotHavePublicSetter()
            .Because(
                "Все классы-сущности в папке/неймспейсе Entities не должны иметь публичных сеттеров (public set;). Разрешены только private set; или init;.")
            .Check(Architecture);
    }

    /// <summary>
    ///     Архитектурный тест, проверяющий, что
    ///     все публичные свойства-коллекции
    ///     в сущностях должны возвращать
    ///     IReadOnlyCollection&lt;T&gt;
    ///     (запрет на отдачу наружу изменяемых List&lt;T&gt;).
    /// </summary>
    [Fact]
    public void DomainEntities_Collections_ShouldBe_ReadOnly()
    {
        GivenClassesConjunction? entities = Classes()
            .That()
            .ResideInNamespaceMatching(@"^PhotoCatalog\.Domain\.Entities($|\..*)")
            .And()
            .AreNotAbstract();

        PropertyMembers()
            .That()
            .ArePublic()
            .And()
            .AreDeclaredIn(entities)
            .Should()
            .FollowCustomCondition(property =>
            {
                string? name = property.Type.FullName;
                bool isCollection = name.Contains("IReadOnlyCollection") ||
                                    name.Contains("List") ||
                                    name.Contains("IEnumerable");
                if (!isCollection)
                {
                    return new ConditionResult(property, true);
                }

                bool isReadOnly = property.Type.FullName.StartsWith("System.Collections.Generic.IReadOnlyCollection`1");

                return new ConditionResult(property, isReadOnly);
            }, "if collection be type IReadOnlyCollection<T>")
            .Because(
                "Все публичные свойства-коллекции в сущностях должны возвращать IReadOnlyCollection<T> (запрет на отдачу наружу изменяемых List<T>).")
            .Check(Architecture);
    }
}