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
        var result = Dimensions.Create(100, 100);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.Width);
        Assert.Equal(100, result.Value.Height);
    }

    /// <summary>
    ///     Проверяет минимально допустимые размеры (1×1 пикселей).
    /// </summary>
    [Fact]
    public void Create_MinimumValidSizeOne_ReturnsSuccess()
    {
        
        Result<Dimensions> result = Dimensions.Create(1, 1);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Width);
        Assert.Equal(1, result.Value.Height);
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
        Assert.Equal(DomainErrors.Dimensions.Invalid, actual.Error);
    }

    /// <summary>
    ///     Проверяет, что превышение максимальных значений (3841×2161) отклоняется.
    /// </summary>
    [Fact]
    public void Create_MaxValues_ReturnsFailure()
    {
        Result<Dimensions> actual = Dimensions.Create(3841, 2161);
        
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actual.Error);
    }

    /// <summary>
    ///     Проверяет, что отрицательные значения отклоняются с ошибкой Invalid.
    /// </summary>
    [Fact]
    public void Create_NegativeValues_ReturnsFailure()
    {
        Result<Dimensions> actual = Dimensions.Create(-1, -1);
        
        Assert.False(actual.IsSuccess);
        Assert.True(actual.IsFailure);
        Assert.Equal(DomainErrors.Dimensions.Invalid, actual.Error);
    }
}