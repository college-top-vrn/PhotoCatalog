using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки методов конвертации и композиции (ToResult, Ensure, ResultVoid.Then).
/// </summary>
public class ResultCompositionTests
{
    private static readonly Error TestError = new("Test.Error", "Тестовая ошибка");
    private const string NullValueErrorCode = "System.NullValue";

    /// <summary>
    ///     Проверяет, что ToResult преобразует обычный объект в успешный результат.
    /// </summary>
    [Fact]
    public void ToResult_WithValidObject_ReturnsSuccess()
    {
        var input = "test data";
        var result = input.ToResult();

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
        var result = input.ToResult();

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
        var result = input.ToResult(TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет переход от успешного ResultVoid к Result{T}.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultT_WhenSuccess_ExecutesNextStep()
    {
        var initial = ResultVoid.Success();
        var result = initial.Then(() => Result<int>.Success(100));

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value);
    }

    /// <summary>
    ///     Проверяет, что при провальном ResultVoid переход к Result{T} не выполняется.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultT_WhenFailure_ReturnsOriginalError()
    {
        var initial = ResultVoid.Failure(TestError);
        var result = initial.Then(() => Result<int>.Success(100));

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет последовательную связь двух ResultVoid.
    /// </summary>
    [Fact]
    public void ResultVoid_Then_ToResultVoid_WhenSuccess_ExecutesNextStep()
    {
        var initial = ResultVoid.Success();
        var result = initial.Then(() => ResultVoid.Success());

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    ///     Проверяет, что Ensure сохраняет успех, если условие истинно.
    /// </summary>
    [Fact]
    public void Ensure_WhenPredicateIsTrue_ReturnsSuccess()
    {
        var initial = Result<int>.Success(10);
        var result = initial.Ensure(v => v > 0, TestError);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value);
    }

    /// <summary>
    ///     Проверяет, что Ensure прерывает цепочку ошибкой, если условие ложно.
    /// </summary>
    [Fact]
    public void Ensure_WhenPredicateIsFalse_ReturnsFailure()
    {
        var initial = Result<int>.Success(-5);
        var result = initial.Ensure(v => v > 0, TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }

    /// <summary>
    ///     Проверяет, что Ensure игнорируется, если результат уже содержит ошибку.
    /// </summary>
    [Fact]
    public void Ensure_WhenInitialIsFailure_ReturnsInitialError()
    {
        var initial = Result<int>.Failure(TestError);
        var newError = new Error("New.Error", "Fail");

        var result = initial.Ensure(v => v > 100, newError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }
}