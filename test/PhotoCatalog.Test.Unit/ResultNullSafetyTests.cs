namespace PhotoCatalog.Test.Unit;

using PhotoCatalog.Domain.Primitives;

using Xunit;

/// <summary>
///     Набор модульных тестов для проверки функциональной безопасности (Null Safety)
///     и деконструкции обновленного класса Result{T}.
/// </summary>
public class ResultNullSafetyTests
{
    private const string NullValueErrorCode = "System.NullValue";

    #region Тесты функциональной деградации (Null Safety)

    /// <summary>
    ///     Проверяет, что явный вызов фабрики успеха с пустым аргументом 
    ///     корректно перехватывается и возвращает системную ошибку провала.
    /// </summary>
    [Fact]
    public void SuccessFactory_WithNullArgument_ReturnsFailureWithSystemError()
    {
        string? nullString = null;


        var result = Result<string>.Success(nullString!);


        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(NullValueErrorCode, result.Error.Code);
    }

    /// <summary>
    ///     Проверяет, что неявное приведение пустой переменной к типу Result{T} 
    ///     корректно перехватывается и возвращает системную ошибку провала.
    /// </summary>
    [Fact]
    public void ImplicitCast_WithNullVariable_ReturnsFailureWithSystemError()
    {
        string? nullString = null;


        Result<string> result = nullString!;


        Assert.False(result.IsSuccess);
        Assert.Equal(NullValueErrorCode, result.Error.Code);
    }

    #endregion

    #region Дополнительные тесты: Деконструкция (Монадный стиль)

    /// <summary>
    ///     Проверяет, что успешный результат корректно разбивается на кортеж переменных.
    /// </summary>
    [Fact]
    public void Deconstruct_OnSuccess_ReturnsCorrectTuple()
    {
        var expectedValue = "Текст";
        var result = Result<string>.Success(expectedValue);


        var (isSuccess, value, error) = result;


        Assert.True(isSuccess);
        Assert.Equal(expectedValue, value);
        Assert.Equal(Error.None, error);
    }

    /// <summary>
    ///     Проверяет, что провальный результат корректно разбивается на кортеж переменных.
    /// </summary>
    [Fact]
    public void Deconstruct_OnFailure_ReturnsCorrectTuple()
    {
        var expectedError = new Error("Test.Code", "Test Message");
        var result = Result<string>.Failure(expectedError);


        var (isSuccess, value, error) = result;


        Assert.False(isSuccess);
        Assert.Null(value); // Значение по умолчанию для ссылочного типа
        Assert.Equal(expectedError, error);
    }

    /// <summary>
    ///     Проверяет деконструкцию значимого типа (struct) при провале.
    ///     Убеждаемся, что value получает default(T), а не вызывает исключений.
    /// </summary>
    [Fact]
    public void Deconstruct_ValueTypeOnFailure_ReturnsDefaultValue()
    {
        var expectedError = new Error("Numeric.Error", "Fail");
        var result = Result<int>.Failure(expectedError);


        var (isSuccess, value, error) = result;


        Assert.False(isSuccess);
        Assert.Equal(0, value);
        Assert.Equal(expectedError, error);
    }

    #endregion
}