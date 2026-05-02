using PhotoCatalog.Domain.Primitives;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Тесты для проверки поведения объекта <see cref="Error" />.
/// </summary>
public class ErrorTest
{
    [Fact]
    public void Errors_WithSameCodeAndMessage_ShouldBeEqual()
    {
        const string expectedCode = "Tag.EmptyName";
        const string expectedMessage = "Имя тега не может быть пустым.";

        Error error1 = new(expectedCode, expectedMessage);
        Error error2 = new(expectedCode, expectedMessage);

        Assert.Equal(error1, error2);
    }

    [Fact]
    public void None_ShouldHaveEmptyCodeAndMessage()
    {
        Error noneError = Error.None;

        Assert.Equal(string.Empty, noneError.Code);
        Assert.Equal(string.Empty, noneError.Message);
    }
}