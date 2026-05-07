
using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для полного покрытия базовых свойств и состояний структуры ResultVoid.
/// </summary>
public class ResultVoidCoreTests
{
    private static readonly Error TestError = new("Core.Error", "Test message");

    /// <summary>
    ///     Проверяет инициализацию успешного состояния ResultVoid.
    /// </summary>
    [Fact]
    public void Success_ShouldInitializeSuccessState()
    {
        var result = ResultVoid.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    /// <summary>
    ///     Проверяет инициализацию провального состояния ResultVoid.
    /// </summary>
    [Fact]
    public void Failure_ShouldInitializeFailureState()
    {
        var result = ResultVoid.Failure(TestError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(TestError, result.Error);
    }
}
