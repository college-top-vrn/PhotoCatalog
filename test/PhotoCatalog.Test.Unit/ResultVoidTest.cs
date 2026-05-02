using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Содержит набор модульных тестов для проверки логики базового контейнера <see cref="ResultVoid" />.
/// </summary>
public class ResultVoidTests
{
    /// <summary>
    ///     Проверяет, что при создании успешного результата без значения
    ///     устанавливаются правильные флаги состояния и пустая ошибка.
    /// </summary>
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue_And_ErrorNone()
    {
        ResultVoid result = ResultVoid.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    /// <summary>
    ///     Проверяет, что провальный результат без значения содержит
    ///     переданную информацию об ошибке и флаг IsFailure.
    /// </summary>
    [Fact]
    public void Failure_WithValidError_ShouldReturnIsFailureTrue_And_MatchError()
    {
        Error expectedError = new("Test.Failure", "Тестовое сообщение об ошибке");

        ResultVoid result = ResultVoid.Failure(expectedError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }
}