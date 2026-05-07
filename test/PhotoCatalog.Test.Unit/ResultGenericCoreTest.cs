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
    public void Success_WithValidValue_ShouldInitializeSuccessState()
    {
        var result = Result<int>.Success(10);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(10, result.Value);
    }

    /// <summary>
    ///     Проверяет защиту от передачи null при инициализации успешного состояния.
    /// </summary>
    [Fact]
    public void Success_WithNullValue_ShouldReturnSystemNullValueError()
    {
        var result = Result<string>.Success(null!);

        Assert.True(result.IsFailure);
        Assert.Equal(SystemErrors.NullValue, result.Error);
    }

    /// <summary>
    ///     Проверяет инициализацию провального состояния.
    /// </summary>
    [Fact]
    public void Failure_ShouldInitializeFailureStateWithDefaultValue()
    {
        var result = Result<Guid>.Failure(TestError);

        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
        Assert.Equal(default, result.Value);
    }

    /// <summary>
    ///     Проверяет неявное приведение обобщенного типа к ResultVoid.
    /// </summary>
    [Fact]
    public void ImplicitOperator_ToResultVoid_ShouldMapStatusCorrectly()
    {
        ResultVoid voidSuccess = Result<int>.Success(100);
        ResultVoid voidFailure = Result<int>.Failure(TestError);

        Assert.True(voidSuccess.IsSuccess);
        Assert.True(voidFailure.IsFailure);
        Assert.Equal(TestError, voidFailure.Error);
    }
}