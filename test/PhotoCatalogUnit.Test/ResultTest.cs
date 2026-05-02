using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalogUnit.Test;

/// <summary>
/// Тесты для проверки поведения базового объекта <see cref="Result"/>.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Проверяет, что вызов <see cref="Result.Success"/> корректно инициализирует успешное состояние: 
    /// флаг <see cref="Result.IsSuccess"/> устанавливается в true, а свойство ошибки содержит <see cref="Error.None"/>.
    /// </summary>
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue_And_ErrorNone()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    /// <summary>
    /// Проверяет, что вызов <see cref="Result.Failure"/> корректно инициализирует провальное состояние: 
    /// флаг <see cref="Result.IsFailure"/> устанавливается в true, а свойство <see cref="Result.Error"/> 
    /// строго соответствует переданному объекту бизнес-ошибки.
    /// </summary>
    [Fact]
    public void Failure_WithValidError_ShouldReturnIsFailureTrue_And_MatchError()
    {
        var expectedError = new Error("Test.Failure", "Тестовое сообщение об ошибке");

        var result = Result.Failure(expectedError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }
}