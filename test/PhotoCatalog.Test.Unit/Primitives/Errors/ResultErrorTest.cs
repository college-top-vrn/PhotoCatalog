using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit.Primitives.Errors;

/// <summary>
///     Тесты для проверки поведения объекта <see cref="ResultError" />.
/// </summary>
public class ResultErrorTest
{
    [Fact]
    public void ErrorsWithSameCodeAndMessageShouldBeEqual()
    {
        const string expectedCode = "Tag.EmptyName";
        const string expectedMessage = "Имя тега не может быть пустым.";

        ResultError error1 = new(expectedCode, expectedMessage);
        ResultError error2 = new(expectedCode, expectedMessage);

        Assert.Equal(error1, error2);
    }

    [Fact]
    public void NoneShouldHaveEmptyCodeAndMessage()
    {
        ResultError noneResultError = ResultError.None;

        Assert.Equal(string.Empty, noneResultError.Code);
        Assert.Equal(string.Empty, noneResultError.Message);
    }
}