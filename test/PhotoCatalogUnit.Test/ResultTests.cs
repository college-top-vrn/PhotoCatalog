namespace PhotoCatalogUnit.Test;

using PhotoCatalog.Domain.Primitives;
using Xunit;


/// <summary>
/// Содержит набор модульных тестов для проверки логики обобщенного контейнера <see cref="Result{T}"/>.
/// </summary>
public class ResultTests
{
    /// <summary>
    /// Проверяет, что фабричный метод <see cref="Result{T}.Success"/> возвращает объект 
    /// в успешном состоянии, содержащий ожидаемое значение и пустую ошибку.
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
    /// Проверяет, что фабричный метод <see cref="Result{T}.Failure"/> возвращает объект 
    /// в состоянии ошибки, где свойство Value принимает значение по умолчанию (default).
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
    /// Проверяет оператор неявного приведения из типа данных <typeparamref name="T"/> 
    /// в <see cref="Result{T}"/>, имитируя успешный возврат значения из доменной логики.
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
    /// Проверяет оператор неявного приведения из <see cref="Error"/> в <see cref="Result{T}"/>, 
    /// имитируя возврат доменной ошибки вместо ожидаемого объекта.
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
    /// Проверяет корректность маппинга типизированного <see cref="Result{T}"/> в базовый <see cref="ResultVoid"/>, 
    /// гарантируя сохранение статуса операции и кода ошибки при потере значения.
    /// </summary>
    [Fact]
    public void ImplicitOperator_ToBaseResult_ShouldMapStateCorrectly()
    {
        var error = new Error("Error", "Message");
        Result<int> successT = 100;
        Result<int> failureT = error;

        ResultVoid baseSuccess = successT;
        ResultVoid baseFailure = failureT;

        Assert.True(baseSuccess.IsSuccess);
        Assert.True(baseFailure.IsFailure);
        Assert.Equal(error, baseFailure.Error);
    }
}