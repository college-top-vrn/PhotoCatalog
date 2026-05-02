using System.Diagnostics;

using Xunit;

using PhotoCatalog.Domain.Primitives;

namespace PhotoCatalogUnit.Test;

/// <summary>
/// Тесты для проверки поведения объекта <see cref="Error"/>.
/// </summary>
public class ErrorTests
{
    [Fact]
    public void Errors_WithSameCodeAndMessage_ShouldBeEqual()
    {
        const string expectedCode = "Tag.EmptyName";
        const string expectedMessage = "Имя тега не может быть пустым.";

        var error1 = new Error(expectedCode, expectedMessage);
        var error2 = new Error(expectedCode, expectedMessage);
        
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void None_ShouldHaveEmptyCodeAndMessage()
    {
        var noneError = Error.None;

        Assert.Equal(string.Empty, noneError.Code);
        Assert.Equal(string.Empty, noneError.Message);
    }
}