using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки методов конвертации и композиции (ToResult, Ensure, ResultVoid.Then).
/// </summary>
public class ResultCompositionTests
{
    private const string NullValueErrorCode = "System.NullValue";
    private static readonly Error TestError = new("Test.Error", "Тестовая ошибка");

    /// <summary>
    ///     Проверяет, что ToResult преобразует обычный объект в успешный результат.
    /// </summary>
    [Fact]
    public void ToResult_WithValidObject_ReturnsSuccess()
    {
        string input = "test data";
        Result<string> result = input.ToResult();

        Assert.True(result.IsSuccess);
        Assert.Equal(input, result.Value);
    }

    /// <summary>
    ///     Проверяет, что ToResult при передаче null возвращает системную ошибку.
    /// </summary>
    [Fact]
    public void ToResult_WithNull_ReturnsSystemFailure()
    {
        string? input = null;
        Result<string?> result = input.ToResult();

        Assert.True(result.IsFailure);
        Assert.Equal(NullValueErrorCode, result.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что ToResult с указанной ошибкой возвращает её, если объект равен null.
    /// </summary>
    [Fact]
    public void ToResult_WithCustomError_ReturnsProvidedErrorOnNull()
    {
        string? input = null;
        Result<string> result = input.ToResult(TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет переход от успешного ResultVoid к Result{T}.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultT_WhenSuccess_ExecutesNextStep()
    {
        ResultVoid initial = ResultVoid.Success();
        Result<int> result = initial.Then(() => Result<int>.Success(100));

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value);
    }

    /// <summary>
    ///     Проверяет, что при провальном ResultVoid переход к Result{T} не выполняется.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultT_WhenFailure_ReturnsOriginalError()
    {
        ResultVoid initial = ResultVoid.Failure(TestError);
        Result<int> result = initial.Then(() => Result<int>.Success(100));

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет последовательную связь двух ResultVoid.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultVoid_WhenSuccess_ExecutesNextStep()
    {
        ResultVoid initial = ResultVoid.Success();
        ResultVoid result = initial.Then(() => ResultVoid.Success());

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    ///     Проверяет, что Ensure сохраняет успех, если условие истинно.
    /// </summary>
    [Fact]
    public void Ensure_WhenPredicateIsTrue_ReturnsSuccess()
    {
        Result<int> initial = Result<int>.Success(10);
        Result<int> result = initial.Ensure(v => v > 0, TestError);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    /// <summary>
    ///     Проверяет, что Ensure прерывает цепочку ошибкой, если условие ложно.
    /// </summary>
    [Fact]
    public void Ensure_WhenPredicateIsFalse_ReturnsFailure()
    {
        Result<int> initial = Result<int>.Success(-5);
        Result<int> result = initial.Ensure(v => v > 0, TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет, что Ensure игнорируется, если результат уже содержит ошибку.
    /// </summary>
    [Fact]
    public void Ensure_WhenInitialIsFailure_ReturnsInitialError()
    {
        Result<int> initial = Result<int>.Failure(TestError);
        Error newError = new("New.Error", "Fail");

        Result<int> result = initial.Ensure(v => v > 100, newError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }
}