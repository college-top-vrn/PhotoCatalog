using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Result;

/// <summary>
///     Тесты для полного покрытия базовых свойств и состояний структуры ResultVoid.
/// </summary>
public class ResultVoidCoreTests
{
    private static readonly ResultError TestResultError = new("Core.Error", "Test message");

    /// <summary>
    ///     Проверяет инициализацию успешного состояния ResultVoid.
    /// </summary>
    [Fact]
    public void SuccessShouldInitializeSuccessState()
    {
        ResultVoid result = ResultVoid.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(ResultError.None, result.ResultError);
    }

    /// <summary>
    ///     Проверяет инициализацию провального состояния ResultVoid.
    /// </summary>
    [Fact]
    public void FailureShouldInitializeFailureState()
    {
        ResultVoid result = ResultVoid.Failure(TestResultError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(TestResultError, result.ResultError);
    }
}