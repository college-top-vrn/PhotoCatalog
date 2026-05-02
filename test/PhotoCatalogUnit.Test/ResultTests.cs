namespace PhotoCatalogUnit.Test;

using PhotoCatalog.Domain.Primitives;

using Xunit;

/// <summary>
/// Тесты для проверки поведения обобщенного объекта <see cref="ResultVoid{T}"/>.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Проверяет, что явный вызов <see cref="ResultVoid{T}.Success"/> корректно устанавливает 
    /// статус успеха, очищает ошибку и сохраняет переданное значение.
    /// </summary>
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue_And_Value()
    {
        string expectedValue = "Тестовая строка";

        var result = ResultVoid<string>.Success(expectedValue);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(expectedValue, result.Value);
    }

    /// <summary>
    /// Проверяет, что явный вызов <see cref="ResultVoid{T}.Failure"/> корректно устанавливает 
    /// статус провала, сохраняет переданную ошибку, а свойство Value безопасно возвращает default(T) 
    /// </summary>
    [Fact]
    public void Failure_ShouldReturnIsFailureTrue_And_ValueShouldBeDefault()
    {
        var expectedError = new Error("Test.Failure", "Тестовая ошибка");

        var result = ResultVoid<string>.Failure(expectedError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Value);
    }

    /// <summary>
    /// Проверяет работу неявного преобразования:
    /// неявное приведение переменной типа T к типу ResultVoid{T} должно автоматически создавать успешный результат.
    /// </summary>
    [Fact]
    public void ImplicitOperator_FromValue_ShouldCreateSuccessResult()
    {
        int expectedValue = 42;

        ResultVoid<int> resultVoid = expectedValue;

        Assert.True(resultVoid.IsSuccess);
        Assert.Equal(expectedValue, resultVoid.Value);
    }

    /// <summary>
    /// Проверяет работу работу неявного преобразования:
    /// неявное приведение объекта Error к типу ResultVoid{T} должно автоматически создавать провальный результат.
    /// </summary>
    [Fact]
    public void ImplicitOperator_FromError_ShouldCreateFailureResult()
    {
        var expectedError = new Error("Code", "Message");

        ResultVoid<int> resultVoid = expectedError;

        Assert.True(resultVoid.IsFailure);
        Assert.Equal(expectedError, resultVoid.Error);
        Assert.Equal(0, resultVoid.Value); // Для значимых типов (int) default — это 0
    }

    /// <summary>
    /// Проверяет работу статического полиморфизма:
    /// неявное приведение обобщенного ResultVoid{T} к базовому нетипизированному ResultVoid
    /// должно корректно переносить статус (IsSuccess/IsFailure) и детали ошибки, отбрасывая Value.
    /// </summary>
    [Fact]
    public void ImplicitOperator_ToBaseResult_ShouldMapStateCorrectly()
    {
        var error = new Error("Error", "Message");
        ResultVoid<int> successT = 100;
        ResultVoid<int> failureT = error;

        ResultVoid baseSuccess = successT;
        ResultVoid baseFailure = failureT;

        Assert.True(baseSuccess.IsSuccess);
        Assert.True(baseFailure.IsFailure);
        Assert.Equal(error, baseFailure.Error);
    }
}