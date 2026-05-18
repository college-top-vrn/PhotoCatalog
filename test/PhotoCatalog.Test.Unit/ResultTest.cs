using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Содержит набор модульных тестов для проверки логики обобщенного контейнера <see cref="Result{T}" />.
/// </summary>
public class ResultTest
{
    /// <summary>
    ///     Проверяет, что фабричный метод <see cref="Result.Success" /> возвращает объект
    ///     в успешном состоянии, содержащий ожидаемое значение и пустую ошибку.
    /// </summary>
    [Fact]
    public void SuccessShouldReturnIsSuccessTrueAndValue()
    {
        const string expectedValue = "Тестовая строка";

        Result<string> result = Result.Success(expectedValue);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(expectedValue, result.Value);
    }

    /// <summary>
    ///     Проверяет, что фабричный метод <see cref="Result.Failure" /> возвращает объект
    ///     в состоянии ошибки, где свойство Value принимает значение по умолчанию (default).
    /// </summary>
    [Fact]
    public void FailureShouldReturnIsFailureTrueAndValueShouldBeDefault()
    {
        Error expectedError = new("Test.Failure", "Тестовая ошибка");

        Result<string> result = Result.Failure<string>(expectedError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Value);
    }
}