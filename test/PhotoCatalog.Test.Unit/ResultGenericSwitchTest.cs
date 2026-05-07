using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки деконструкции и выражений switch для Result{T}.
/// </summary>
public class ResultGenericSwitchTests
{
    private static readonly Error SwitchError = new("Auth.Failed", "Invalid credentials");

    /// <summary>
    ///     Проверяет извлечение значения из успешного результата через Tuple Pattern.
    /// </summary>
    [Fact]
    public void Switch_DeconstructPattern_OnSuccess_ShouldExtractValue()
    {
        var result = Result<int>.Success(42);

        var output = result switch
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
    public void Switch_DeconstructWithWhenClause_ShouldApplyConditionsCorrectly()
    {
        var result = Result<int>.Success(150);

        var category = result switch
        {
            (true, var val, _) when val > 100 => "High",
            (true, var val, _) when val <= 100 => "Low",
            (false, _, _) => "Error",
            _ => "Unknown"
        };

        Assert.Equal("High", category);
    }

    /// <summary>
    ///     Проверяет сопоставление с null для предотвращения NullReferenceException.
    /// </summary>
    [Fact]
    public void Switch_NullCheck_ShouldHandleNullReferenceGracefully()
    {
        Result<string>? nullResult = null;

        var status = nullResult switch
        {
            null => "IsNull",
            { IsSuccess: true } => "IsSuccess",
            { IsFailure: true } => "IsFailure"
        };

        Assert.Equal("IsNull", status);
    }
}