using System;

using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Result;

/// <summary>
///     Тесты для покрытия базовых состояний и операторов обобщенного класса Result{T}.
/// </summary>
public class ResultGenericCoreTests
{
    private static readonly ResultError TestResultError = new("Core.Error", "Test message");

    /// <summary>
    ///     Проверяет инициализацию успешного состояния с валидным значением.
    /// </summary>
    [Fact]
    public void SuccessWithValidValueShouldInitializeSuccessState()
    {
        // TODO: Исправить магические числа
        Result<int> result = PhotoCatalog.Domain.Primitives.Result.Success(10);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultError.None, result.ResultError);
        Assert.Equal(10, result.Value);
    }

    /// <summary>
    ///     Проверяет защиту от передачи null при инициализации успешного состояния.
    /// </summary>
    [Fact]
    public void SuccessWithNullValueShouldReturnSystemNullValueError()
    {
        Result<string> result = PhotoCatalog.Domain.Primitives.Result.Success<string>(null!);

        Assert.True(result.IsFailure);
        Assert.Equal(SystemErrors.NullValue, result.ResultError);
    }

    /// <summary>
    ///     Проверяет инициализацию провального состояния.
    /// </summary>
    [Fact]
    public void FailureShouldInitializeFailureStateWithDefaultValue()
    {
        Result<Guid> result = PhotoCatalog.Domain.Primitives.Result.Failure<Guid>(TestResultError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestResultError, result.ResultError);
        Assert.Equal(Guid.Empty, result.Value);
    }

    /// <summary>
    ///     Проверяет неявное приведение обобщенного типа к ResultVoid.
    /// </summary>
    [Fact]
    public void ImplicitOperatorToResultVoidShouldMapStatusCorrectly()
    {
        // TODO: Исправить магические числа
        ResultVoid voidSuccess = PhotoCatalog.Domain.Primitives.Result.Success(100);
        ResultVoid voidFailure = PhotoCatalog.Domain.Primitives.Result.Failure<int>(TestResultError);

        Assert.True(voidSuccess.IsSuccess);
        Assert.True(voidFailure.IsFailure);
        Assert.Equal(TestResultError, voidFailure.ResultError);
    }
}