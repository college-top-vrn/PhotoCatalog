using System.Collections.Generic;
using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;

using PhotoCatalog.Domain.Interfaces.Repositories;

using Xunit;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace PhotoCatalog.Test.Architectural;

/// <summary>
///     Содержит архитектурные тесты для проверки соблюдения единого стиля кодирования.
/// </summary>
/// <remarks>
///     Тесты проверяют соответствие именований пространств имен, классов и интерфейсов
///     правилам PascalCase, использованию только латинских символов и отсутствию спецсимволов.
/// </remarks>
public class NamingConventions
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(IPhotoRepository).Assembly
        ).Build();

    /// <summary>
    ///     Проверяет, что все пространства имен соответствуют PascalCase и содержат только латинские символы.
    /// </summary>
    /// <remarks>
    ///     Правила проверки:
    ///     <list type="bullet">
    ///         <item><description>Каждый сегмент пространства имен должен начинаться с заглавной буквы</description></item>
    ///         <item><description>Допускаются только латинские буквы (A-Z, a-z) и цифры (0-9)</description></item>
    ///         <item><description>Запрещена кириллица и любые другие символы</description></item>
    ///         <item><description>Запрещены пробелы, подчеркивания (_), дефисы (-) и другие спецсимволы</description></item>
    ///     </list>
    /// </remarks>
    [Fact]
    public void Namespaces_Should_BeInPascalCase_And_ContainOnlyLatinCharacters()
    {
        var violations = new List<string>();

        var types = Types().GetObjects(Architecture);

        foreach (var type in types)
        {
            var namespaceName = type.Namespace?.FullName;

            if (string.IsNullOrEmpty(namespaceName))
                continue;

            var segments = namespaceName.Split('.');

            foreach (var segment in segments)
            {
                if (!Regex.IsMatch(segment, @"^[A-Z][a-zA-Z0-9]*$"))
                {
                    violations.Add(namespaceName);
                    break;
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    ///     Проверяет, что имена всех классов не содержат кириллицу и спецсимволы.
    /// </summary>
    /// <remarks>
    ///     Имена классов должны соответствовать формату PascalCase:
    ///     начинаться с заглавной буквы, содержать только латинские буквы и цифры.
    ///     Generic-типы исключаются из проверки.
    /// </remarks>
    [Fact]
    public void Classes_Should_Not_Contain_Cyrillic_Or_SpecialCharacters()
    {
        // Регулярное выражение также допускает символ ` (backtick) и цифры для generic-типов
        Classes()
            .That()
            .AreNot(typeof(object))
            .Should()
            .HaveNameMatching(@"^[A-Z][a-zA-Z0-9`]*$")
            .Because("Имена классов должны соответствовать PascalCase")
            .Check(Architecture);
    }

    /// <summary>
    ///     Проверяет, что имена всех интерфейсов соответствуют конвенции.
    /// </summary>
    /// <remarks>
    ///     Интерфейсы должны:
    ///     <list type="bullet">
    ///         <item><description>Начинаться с префикса "I"</description></item>
    ///         <item><description>Содержать заглавную букву после префикса</description></item>
    ///         <item><description>Соответствовать формату PascalCase</description></item>
    ///         <item><description>Содержать только латинские буквы и цифры</description></item>
    ///     </list>
    ///     Generic-интерфейсы исключаются из проверки.
    /// </remarks>
    [Fact]
    public void Interfaces_Should_StartWith_I_And_BeInPascalCase()
    {
        Interfaces()
            .That()
            .AreNot(typeof(System.IDisposable))
            .Should()
            .HaveNameMatching(@"^I[A-Z][a-zA-Z0-9`]*$")
            .Because("Интерфейсы должны начинаться с I и соответствовать PascalCase")
            .Check(Architecture);
    }
}