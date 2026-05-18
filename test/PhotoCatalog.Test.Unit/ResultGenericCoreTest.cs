using System;

using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для покрытия базовых состояний и операторов обобщенного класса Result{T}.
/// </summary>
public class ResultGenericCoreTests
{
    private static readonly Error TestError = new("Core.Error", "Test message");

    /// <summary>
    ///     Проверяет инициализацию успешного состояния с валидным значением.
    /// </summary>
    [Fact]
    public void SuccessWithValidValueShouldInitializeSuccessState()
    {
        // TODO: Исправить магические числа
        Result<int> result = Result.Success(10);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(10, result.Value);
    }

    /// <summary>
    ///     Проверяет защиту от передачи null при инициализации успешного состояния.
    /// </summary>
    [Fact]
    public void SuccessWithNullValueShouldReturnSystemNullValueError()
    {
        Result<string> result = Result.Success<string>(null!);

        Assert.True(result.IsFailure);
        Assert.Equal(SystemErrors.NullValue, result.Error);
    }

    /// <summary>
    ///     Проверяет инициализацию провального состояния.
    /// </summary>
    [Fact]
    public void FailureShouldInitializeFailureStateWithDefaultValue()
    {
        Result<Guid> result = Result.Failure<Guid>(TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
        Assert.Equal(Guid.Empty, result.Value);
    }

    /// <summary>
    ///     Проверяет неявное приведение обобщенного типа к ResultVoid.
    /// </summary>
    [Fact]
    public void ImplicitOperatorToResultVoidShouldMapStatusCorrectly()
    {
        // TODO: Исправить магические числа
        ResultVoid voidSuccess = Result.Success(100);
        ResultVoid voidFailure = Result.Failure<int>(TestError);

        Assert.True(voidSuccess.IsSuccess);
        Assert.True(voidFailure.IsFailure);
        Assert.Equal(TestError, voidFailure.Error);
    }
}