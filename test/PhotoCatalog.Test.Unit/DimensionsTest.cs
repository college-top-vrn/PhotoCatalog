using PhotoCatalog.Domain.Primitives;
using PhotoCatalog.Domain.ValueObjects;

using Xunit;

namespace PhotoCatalog.Test.Unit;

/// <summary>
///     Unit-тесты для DimensionsTest
/// </summary>
public class DimensionsTest
{
    /// <summary>
    ///     Проверяет, что валидные размеры (в пределах 1-3840×1-2160) создаются успешно.
    /// </summary>
    [Fact]
    public void Create_ValidWidthAndHeight_ReturnsSuccess()
    {
        // Arrange & Act
        Result<Dimensions> actual = Dimensions.Create(100, 100);
        Result<Dimensions> expectation = Dimensions.Create(100, 100);

        // Assert
        Assert.Equal(expectation, actual);
    }

    /// <summary>
    ///     Проверяет минимально допустимые размеры (1×1 пикселей).
    /// </summary>
    [Fact]
    public void Create_MinimumValidSizeOne_ReturnsSuccess()
    {
        // Arrange & Act
        Result<Dimensions> actual = Dimensions.Create(1, 1);
        Result<Dimensions> expectation = Dimensions.Create(1, 1);

        // Assert
        Assert.Equal(expectation, actual);
    }

    /// <summary>
    ///     Проверяет, что нулевые значения (0×0) отклоняются с ошибкой Invalid.
    /// </summary>
    [Fact]
    public void Create_ZeroValues_ReturnsFailure()
    {
        // Act
        Result<Dimensions> actual = Dimensions.Create(0, 0);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(Result<Dimensions>.Failure(DomainErrors.Dimensions.Invalid), actual.Error);
    }

    /// <summary>
    ///     Проверяет, что превышение максимальных значений (3841×2161) отклоняется.
    /// </summary>
    [Fact]
    public void Create_MaxValues_ReturnsFailure()
    {
        // Act
        Result<Dimensions> actual = Dimensions.Create(3841, 2161);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(Result<Dimensions>.Failure(DomainErrors.Dimensions.Invalid), actual.Error);
    }

    /// <summary>
    ///     Проверяет, что отрицательные значения отклоняются с ошибкой Invalid.
    /// </summary>
    [Fact]
    public void Create_NegativeValues_ReturnsFailure()
    {
        // Act
        Result<Dimensions> actual = Dimensions.Create(-1, -1);

        // Assert
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(Result<Dimensions>.Failure(DomainErrors.Dimensions.Invalid), actual.Error);
    }
}