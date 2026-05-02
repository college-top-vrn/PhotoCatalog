namespace PhotoCatalogUnit.Test;

using PhotoCatalog.Domain.Primitives;
using Xunit;


/// <summary>
/// Тесты для проверки поведения обобщенного объекта <see cref="Result{T}"/>.
/// </summary>
public class ResultTTests
{
    /// <summary>
    /// Проверяет, что явный вызов <see cref="Result{T}.Success"/> корректно устанавливает 
    /// статус успеха, очищает ошибку и сохраняет переданное значение.
    /// </summary>
    [Fact]
    public void Success_ShouldReturnIsSuccessTrue_And_Value()
    {
        string expectedValue = "Тестовая строка";

        var result = Result<string>.Success(expectedValue);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(expectedValue, result.Value);
    }

    /// <summary>
    /// Проверяет, что явный вызов <see cref="Result{T}.Failure"/> корректно устанавливает 
    /// статус провала, сохраняет переданную ошибку, а свойство Value безопасно возвращает default(T) 
    /// </summary>
    [Fact]
    public void Failure_ShouldReturnIsFailureTrue_And_ValueShouldBeDefault()
    {
        var expectedError = new Error("Test.Failure", "Тестовая ошибка");

        var result = Result<string>.Failure(expectedError);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
        Assert.Null(result.Value); 
    }

    /// <summary>
    /// Проверяет работу неявного преобразования:
    /// неявное приведение переменной типа T к типу Result{T} должно автоматически создавать успешный результат.
    /// </summary>
    [Fact]
    public void ImplicitOperator_FromValue_ShouldCreateSuccessResult()
    {
        int expectedValue = 42;

        Result<int> result = expectedValue;

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedValue, result.Value);
    }

    /// <summary>
    /// Проверяет работу работу неявного преобразования:
    /// неявное приведение объекта Error к типу Result{T} должно автоматически создавать провальный результат.
    /// </summary>
    [Fact]
    public void ImplicitOperator_FromError_ShouldCreateFailureResult()
    {
        var expectedError = new Error("Code", "Message");

        Result<int> result = expectedError;

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
        Assert.Equal(0, result.Value); // Для значимых типов (int) default — это 0
    }

    /// <summary>
    /// Проверяет работу статического полиморфизма:
    /// неявное приведение обобщенного Result{T} к базовому нетипизированному Result
    /// должно корректно переносить статус (IsSuccess/IsFailure) и детали ошибки, отбрасывая Value.
    /// </summary>
    [Fact]
    public void ImplicitOperator_ToBaseResult_ShouldMapStateCorrectly()
    {
        var error = new Error("Error", "Message");
        Result<int> successT = 100;
        Result<int> failureT = error;

        Result baseSuccess = successT;
        Result baseFailure = failureT;

        Assert.True(baseSuccess.IsSuccess);
        Assert.True(baseFailure.IsFailure);
        Assert.Equal(error, baseFailure.Error);
    }
}