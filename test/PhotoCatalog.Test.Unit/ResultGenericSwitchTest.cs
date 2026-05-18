using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки деконструкции и выражений switch для Result{T}.
/// </summary>
public class ResultGenericSwitchTests
{
    /// <summary>
    ///     Проверяет извлечение значения из успешного результата через Tuple Pattern.
    /// </summary>
    [Fact]
    public void SwitchDeconstructPatternOnSuccessShouldExtractValue()
    {
        // TODO: Исправить магические числа
        Result<int> result = Result.Success(42);

        string output = result switch
        {
            (true, var val, _) => $"Value is {val}",
            (false, _, var err) => $"Error is {err.Code}",
            _ => "Unknown"
        };

        Assert.Equal("Value is 42", output);
    }

    /// <summary>
    ///     Проверяет использование дополнительных условий (when clause) совместно с деконструктором.
    /// </summary>
    [Fact]
    public void SwitchDeconstructWithWhenClauseShouldApplyConditionsCorrectly()
    {
        Result<int> result = Result.Success(150);

        string category = result switch
        {
            (true, > 100, _) => "High",
            (true, <= 100, _) => "Low",
            (false, _, _) => "Error",
            _ => "Unknown"
        };

        Assert.Equal("High", category);
    }

    /// <summary>
    ///     Проверяет сопоставление с null для предотвращения NullReferenceException.
    /// </summary>
    [Fact]
    public void SwitchNullCheckShouldHandleNullReferenceGracefully()
    {
        Result<string>? nullResult = null;

        string status = nullResult switch
        {
            null => "IsNull",
            { IsSuccess: true } => "IsSuccess",
            { IsFailure: true } => "IsFailure",
            _ => "IsNull"
        };

        Assert.Equal("IsNull", status);
    }
}