using System;

using PhotoCatalog.Domain.Extensions;
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Набор модульных тестов для проверки логики Fluent API.
/// </summary>
/// <remarks>
///     Тесты проверяют корректность передачи состояния (успех/провал) по цепочке вычислений
///     и гарантируют защиту от исключений <see cref="NullReferenceException"/>.
/// </remarks>
public class ResultExtensionsTests
{
    private static readonly Error TestError = new("Test.Error", "Тестовая ошибка");

    private const string NullErrorCode = "System.NullResult";

    #region Then (Result -> Result)

    /// <summary>
    ///     Проверяет, что при успешном результате выполняется следующий шаг, 
    ///     и возвращается его результат.
    /// </summary>
    [Fact]
    public void Then_WhenResultIsSuccess_ExecutesNextStep()
    {
        var initialResult = Result<int>.Success(10);
        Func<int, Result<string>> nextStep = val => Result<string>.Success($"Значение: {val}");

        var finalResult = initialResult.Then(nextStep);

        Assert.True(finalResult.IsSuccess);
        Assert.Equal("Значение: 10", finalResult.Value);
    }

    /// <summary>
    ///     Проверяет, что при провальном результате следующий шаг игнорируется, 
    ///     а ошибка передается дальше.
    /// </summary>
    [Fact]
    public void Then_WhenResultIsFailure_ReturnsOriginalErrorAndSkipsNextStep()
    {
        var initialResult = Result<int>.Failure(TestError);
        bool isNextStepCalled = false;

        Func<int, Result<string>> nextStep = val =>
        {
            isNextStepCalled = true;
            return Result<string>.Success("Успех");
        };

        var finalResult = initialResult.Then(nextStep);

        Assert.False(finalResult.IsSuccess);
        Assert.Equal(TestError, finalResult.Error);
        Assert.False(isNextStepCalled);
    }

    /// <summary>
    ///     Проверяет защиту от NullReferenceException.
    /// </summary>
    [Fact]
    public void Then_WhenResultIsNull_ReturnsSystemNullError()
    {
        Result<int>? initialResult = null;

        var finalResult = initialResult.Then(val => Result<string>.Success("Ok"));

        Assert.False(finalResult.IsSuccess);
        Assert.Equal(NullErrorCode, finalResult.Error.Code);
    }

    #endregion

    #region Transform

    /// <summary>
    ///     Проверяет, что успешное значение корректно преобразуется в новый тип.
    /// </summary>
    [Fact]
    public void Transform_WhenResultIsSuccess_ReturnsMappedValue()
    {
        var initialResult = Result<int>.Success(5);

        var finalResult = initialResult.Transform(val => val * 2);

        Assert.True(finalResult.IsSuccess);
        Assert.Equal(10, finalResult.Value);
    }

    #endregion

    #region OnSuccess / OnFailure

    /// <summary>
    ///     Проверяет, что делегат OnSuccess выполняется только при успехе.
    /// </summary>
    [Fact]
    public void OnSuccess_WhenResultIsSuccess_ExecutesAction()
    {
        var initialResult = Result<int>.Success(1);
        int sideEffectValue = 0;

        initialResult.OnSuccess(val => sideEffectValue = val);

        Assert.Equal(1, sideEffectValue);
    }

    /// <summary>
    ///     Проверяет, что делегат OnFailure выполняется только при наличии ошибки.
    /// </summary>
    [Fact]
    public void OnFailure_WhenResultIsFailure_ExecutesActionWithError()
    {
        var initialResult = Result<int>.Failure(TestError);
        Error capturedError = Error.None;

        initialResult.OnFailure(err => capturedError = err);

        Assert.Equal(TestError, capturedError);
    }

    #endregion

    #region Finally

    /// <summary>
    ///     Проверяет, что метод Finally вызывает функцию успеха для валидных данных.
    /// </summary>
    [Fact]
    public void Finally_WhenResultIsSuccess_ReturnsSuccessFuncResult()
    {
        var initialResult = Result<int>.Success(200);

        string output = initialResult.Finally(
            success: val => $"OK: {val}",
            failure: err => "FAIL"
        );

        Assert.Equal("OK: 200", output);
    }

    /// <summary>
    ///     Проверяет, что метод Finally вызывает функцию ошибки при провале.
    /// </summary>
    [Fact]
    public void Finally_WhenResultIsFailure_ReturnsFailureFuncResult()
    {
        var initialResult = Result<int>.Failure(TestError);

        string output = initialResult.Finally(
            success: val => "OK",
            failure: err => $"FAIL: {err.Code}"
        );

        Assert.Equal("FAIL: Test.Error", output);
    }

    #endregion
}